-- -----------------------------------------------------------------------------
-- 1. PROFILES & AUTHENTICATION
-- -----------------------------------------------------------------------------

-- Create a table for public profiles (linked to auth.users)
CREATE TABLE IF NOT EXISTS public.profiles (
    id UUID REFERENCES auth.users(id) ON DELETE CASCADE PRIMARY KEY,
    email TEXT,
    role TEXT DEFAULT 'student', -- 'student', 'driver', 'admin'
    full_name TEXT,
    avatar_url TEXT,
    student_id TEXT, -- For students
    ic_number TEXT,  -- For drivers
    updated_at TIMESTAMP WITH TIME ZONE
);

-- Enable Row Level Security (RLS)
ALTER TABLE public.profiles ENABLE ROW LEVEL SECURITY;

-- Drop exist policies to avoid errors
DROP POLICY IF EXISTS "Public profiles are viewable by everyone" ON public.profiles;
DROP POLICY IF EXISTS "Users can insert their own profile" ON public.profiles;
DROP POLICY IF EXISTS "Users can update own profile" ON public.profiles;

-- Policy: Public profiles are viewable by everyone
CREATE POLICY "Public profiles are viewable by everyone" 
ON public.profiles FOR SELECT USING (true);

-- Policy: Users can insert their own profile
CREATE POLICY "Users can insert their own profile" 
ON public.profiles FOR INSERT WITH CHECK (auth.uid() = id);

-- Policy: Users can update their own profile
CREATE POLICY "Users can update own profile" 
ON public.profiles FOR UPDATE USING (auth.uid() = id);

-- Function to handle new user registration automatically
CREATE OR REPLACE FUNCTION public.handle_new_user() 
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO public.profiles (id, email, role, full_name, student_id, ic_number)
    VALUES (
        new.id, 
        new.email, 
        COALESCE(new.raw_user_meta_data->>'role', 'student'),
        new.raw_user_meta_data->>'full_name',
        new.raw_user_meta_data->>'student_id',
        new.raw_user_meta_data->>'ic_number'
    );
    RETURN new;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Trigger to call the function on new user creation
DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
CREATE TRIGGER on_auth_user_created
    AFTER INSERT ON auth.users
    FOR EACH ROW EXECUTE FUNCTION public.handle_new_user();

-- Ensure columns exist if table was already there
ALTER TABLE public.profiles ADD COLUMN IF NOT EXISTS student_id text;
ALTER TABLE public.profiles ADD COLUMN IF NOT EXISTS ic_number text;
ALTER TABLE public.profiles ADD COLUMN IF NOT EXISTS avatar_url text;


-- -----------------------------------------------------------------------------
-- 2. ANNOUNCEMENTS
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS public.announcements (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    title TEXT NOT NULL,
    content TEXT NOT NULL,
    is_urgent BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

ALTER TABLE public.announcements ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS "Everyone can read announcements" ON public.announcements;
DROP POLICY IF EXISTS "Admins can manage announcements" ON public.announcements;

-- Announcements: Everyone can read, only Admins can manage
CREATE POLICY "Everyone can read announcements" ON public.announcements FOR SELECT USING (true);

CREATE POLICY "Admins can manage announcements" ON public.announcements FOR ALL USING (
    EXISTS (SELECT 1 FROM public.profiles WHERE id = auth.uid() AND role = 'admin')
);


-- -----------------------------------------------------------------------------
-- 3. BUS SCHEDULES
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS public.schedules (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    route_name TEXT NOT NULL,      -- e.g., "Route A"
    departure_time TIME NOT NULL,  -- e.g., "08:00:00"
    day_type TEXT DEFAULT 'Daily', -- e.g., "Daily", "Weekend"
    status TEXT DEFAULT 'On Time'  -- 'On Time', 'Delayed', 'Cancelled'
);

ALTER TABLE public.schedules ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS "Everyone can read schedules" ON public.schedules;
DROP POLICY IF EXISTS "Admins can manage schedules" ON public.schedules;

-- Schedules: Everyone can read, only Admins can manage
CREATE POLICY "Everyone can read schedules" ON public.schedules FOR SELECT USING (true);

CREATE POLICY "Admins can manage schedules" ON public.schedules FOR ALL USING (
    EXISTS (SELECT 1 FROM public.profiles WHERE id = auth.uid() AND role = 'admin')
);


-- -----------------------------------------------------------------------------
-- 4. BUS LOCATIONS (REALTIME)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS public.bus_locations (
    driver_id UUID REFERENCES auth.users(id) PRIMARY KEY, -- One location per driver
    route_name TEXT,
    latitude DOUBLE PRECISION,
    longitude DOUBLE PRECISION,
    status TEXT, -- 'Driving', 'Resting'
    last_updated TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now())
);

ALTER TABLE public.bus_locations ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS "Everyone can see buses" ON public.bus_locations;
DROP POLICY IF EXISTS "Drivers can update own location" ON public.bus_locations;

-- Bus Locations: Everyone can read, Drivers can update their own
CREATE POLICY "Everyone can see buses" ON public.bus_locations FOR SELECT USING (true);

CREATE POLICY "Drivers can update own location" ON public.bus_locations FOR ALL USING (auth.uid() = driver_id);


-- -----------------------------------------------------------------------------
-- 5. TRIP PASSENGERS (Optional - For tracking who is on board)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS public.trip_passengers (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    trip_id TEXT, -- Can be linked to a schedule or just a timestamped run
    student_id UUID REFERENCES public.profiles(id),
    boarded_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now())
);

ALTER TABLE public.trip_passengers ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS "Students can board" ON public.trip_passengers;
DROP POLICY IF EXISTS "Everyone can read passengers" ON public.trip_passengers;

-- Trip Passengers: 
-- Students can insert themselves (boarding)
CREATE POLICY "Students can board" ON public.trip_passengers FOR INSERT WITH CHECK (auth.uid() = student_id);
-- Everyone can read (for Admin monitoring and maybe driver)
CREATE POLICY "Everyone can read passengers" ON public.trip_passengers FOR SELECT USING (true);


-- -----------------------------------------------------------------------------
-- 6. BOOKINGS (NEW)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS public.bookings (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    schedule_id UUID REFERENCES public.schedules(id) ON DELETE CASCADE,
    student_id UUID REFERENCES public.profiles(id) ON DELETE CASCADE,
    booking_date DATE DEFAULT CURRENT_DATE,
    status TEXT DEFAULT 'Booked', -- 'Booked', 'Boarded', 'No Show'
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    UNIQUE(schedule_id, student_id, booking_date)
);

ALTER TABLE public.bookings ENABLE ROW LEVEL SECURITY;

-- Drop existing policies to prevent dupes if rerunning
DROP POLICY IF EXISTS "Students can manage own bookings" ON public.bookings;
DROP POLICY IF EXISTS "Drivers and Admins can view bookings" ON public.bookings;
DROP POLICY IF EXISTS "Drivers can update bookings" ON public.bookings;

-- Students can manage their own bookings (Insert, Select, Delete)
CREATE POLICY "Students can manage own bookings" ON public.bookings
    FOR ALL USING (auth.uid() = student_id);

-- Drivers and Admins can view all bookings
CREATE POLICY "Drivers and Admins can view bookings" ON public.bookings
    FOR SELECT USING (
        EXISTS (SELECT 1 FROM public.profiles WHERE id = auth.uid() AND role IN ('driver', 'admin'))
    );

-- Drivers can update booking status (e.g. check-in)
CREATE POLICY "Drivers can update bookings" ON public.bookings
    FOR UPDATE USING (
        EXISTS (SELECT 1 FROM public.profiles WHERE id = auth.uid() AND role = 'driver')
    );


-- -----------------------------------------------------------------------------
-- 7. SEED DATA (INITIAL CONTENT)
-- -----------------------------------------------------------------------------

-- Clear existing data (Optional, be careful in production)
-- TRUNCATE public.announcements, public.schedules; 

-- Seed Announcements
INSERT INTO public.announcements (title, content, is_urgent)
SELECT 'Exam Week Bus Schedule', 'Extra buses will potentially be added during exam week.', true
WHERE NOT EXISTS (SELECT 1 FROM public.announcements WHERE title = 'Exam Week Bus Schedule');

INSERT INTO public.announcements (title, content, is_urgent)
SELECT 'App Maintenance', 'Maintenance scheduled for Sunday 2 AM - 4 AM.', false
WHERE NOT EXISTS (SELECT 1 FROM public.announcements WHERE title = 'App Maintenance');

-- Seed Schedules
INSERT INTO public.schedules (route_name, departure_time, day_type)
SELECT 'Route A (Hostel -> Campus)', '07:30:00', 'Daily'
WHERE NOT EXISTS (SELECT 1 FROM public.schedules WHERE departure_time = '07:30:00');

INSERT INTO public.schedules (route_name, departure_time, day_type)
SELECT 'Route B (Campus -> Hostel)', '07:45:00', 'Daily'
WHERE NOT EXISTS (SELECT 1 FROM public.schedules WHERE departure_time = '07:45:00');

INSERT INTO public.schedules (route_name, departure_time, day_type)
SELECT 'Route A (Hostel -> Campus)', '08:30:00', 'Daily'
WHERE NOT EXISTS (SELECT 1 FROM public.schedules WHERE departure_time = '08:30:00');

INSERT INTO public.schedules (route_name, departure_time, day_type)
SELECT 'Route B (Campus -> Hostel)', '08:45:00', 'Daily'
WHERE NOT EXISTS (SELECT 1 FROM public.schedules WHERE departure_time = '08:45:00');


-- -----------------------------------------------------------------------------
-- 8. STORAGE BUCKETS
-- -----------------------------------------------------------------------------
-- Note: Requires Supabase Storage properly enabled.
-- Inserting into storage.buckets creates a new public bucket named 'avatars'
INSERT INTO storage.buckets (id, name, public) 
VALUES ('avatars', 'avatars', true)
ON CONFLICT (id) DO NOTHING;

-- Policies for Storage (Simple public access, auth upload)
DROP POLICY IF EXISTS "Avatar images are publicly accessible" ON storage.objects;
DROP POLICY IF EXISTS "Users can upload their own avatar" ON storage.objects;

-- Make avatars public
CREATE POLICY "Avatar images are publicly accessible" ON storage.objects
  FOR SELECT USING ( bucket_id = 'avatars' );

-- Allow authenticated users to upload their own avatar 
-- (Checks matches path with user id folder, secure way)
CREATE POLICY "Users can upload their own avatar" ON storage.objects
  FOR INSERT WITH CHECK (
    bucket_id = 'avatars' AND 
    auth.role() = 'authenticated' AND
    (storage.foldername(name))[1] = auth.uid()::text
  );

-- 1. Create Announcements Table
CREATE TABLE IF NOT EXISTS public.announcements (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    title TEXT NOT NULL,
    content TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    is_urgent BOOLEAN DEFAULT FALSE
);

-- 2. Create Schedules Table
CREATE TABLE IF NOT EXISTS public.schedules (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    route_name TEXT NOT NULL,
    departure_time TIME NOT NULL,
    day_type TEXT DEFAULT 'Weekday', -- 'Weekday', 'Weekend'
    status TEXT DEFAULT 'Scheduled' -- 'Scheduled', 'Delayed', 'Cancelled'
);

-- 3. Create Trip Passengers Table (for tracking students on buses)
CREATE TABLE IF NOT EXISTS public.trip_passengers (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    trip_id UUID REFERENCES public.active_trips(id) ON DELETE CASCADE,
    student_id UUID REFERENCES public.profiles(id) ON DELETE CASCADE,
    boarded_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now())
);

-- 4. Enable RLS
ALTER TABLE public.announcements ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.schedules ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.trip_passengers ENABLE ROW LEVEL SECURITY;

-- 5. RLS Policies

-- Announcements: Everyone can read, only Admins can insert/update/delete
CREATE POLICY "Everyone can read announcements" ON public.announcements FOR SELECT USING (true);
CREATE POLICY "Admins can manage announcements" ON public.announcements FOR ALL USING (
    EXISTS (SELECT 1 FROM public.profiles WHERE id = auth.uid() AND role = 'admin')
);

-- Schedules: Everyone can read, only Admins can manage
CREATE POLICY "Everyone can read schedules" ON public.schedules FOR SELECT USING (true);
CREATE POLICY "Admins can manage schedules" ON public.schedules FOR ALL USING (
    EXISTS (SELECT 1 FROM public.profiles WHERE id = auth.uid() AND role = 'admin')
);

-- Trip Passengers: 
-- Students can insert themselves (boarding)
CREATE POLICY "Students can board" ON public.trip_passengers FOR INSERT WITH CHECK (auth.uid() = student_id);
-- Everyone can read (for Admin monitoring and maybe driver)
CREATE POLICY "Everyone can read passengers" ON public.trip_passengers FOR SELECT USING (true);

-- 6. Seed Initial Data (Optional)
INSERT INTO public.schedules (route_name, departure_time, day_type) VALUES
('Route A', '07:30', 'Weekday'),
('Route A', '08:30', 'Weekday'),
('Route B', '07:45', 'Weekday'),
('Route B', '08:45', 'Weekday');

INSERT INTO public.announcements (title, content, is_urgent) VALUES
('Welcome to UTS Transit', 'The new app is live! Check schedules and track buses.', FALSE),
('Maintenance Notice', 'Bus A will be under maintenance this Sunday.', TRUE);

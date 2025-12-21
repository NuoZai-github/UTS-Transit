-- =============================================================================
-- UTS TRANSIT APP - 完整数据库设置
-- 复制全部代码到 Supabase SQL Editor 运行
-- =============================================================================

-- -----------------------------------------------------------------------------
-- 1. PROFILES (用户资料)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS public.profiles (
    id UUID REFERENCES auth.users(id) ON DELETE CASCADE PRIMARY KEY,
    email TEXT,
    role TEXT DEFAULT 'student',
    full_name TEXT,
    avatar_url TEXT,
    student_id TEXT,
    ic_number TEXT,
    updated_at TIMESTAMP WITH TIME ZONE
);

-- 确保列存在
ALTER TABLE public.profiles ADD COLUMN IF NOT EXISTS student_id TEXT;
ALTER TABLE public.profiles ADD COLUMN IF NOT EXISTS ic_number TEXT;
ALTER TABLE public.profiles ADD COLUMN IF NOT EXISTS avatar_url TEXT;
ALTER TABLE public.profiles ADD COLUMN IF NOT EXISTS full_name TEXT;
ALTER TABLE public.profiles ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP WITH TIME ZONE;

-- 授权
GRANT ALL ON TABLE public.profiles TO postgres;
GRANT ALL ON TABLE public.profiles TO service_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public.profiles TO authenticated;
GRANT SELECT ON TABLE public.profiles TO anon;

-- RLS
ALTER TABLE public.profiles ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS "Public profiles are viewable by everyone" ON public.profiles;
DROP POLICY IF EXISTS "Users can manage own profile" ON public.profiles;

CREATE POLICY "Public profiles are viewable by everyone" 
ON public.profiles FOR SELECT USING (true);

CREATE POLICY "Users can manage own profile" 
ON public.profiles FOR ALL 
USING (auth.uid() = id) 
WITH CHECK (auth.uid() = id);

-- 自动创建用户资料的触发器
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
    )
    ON CONFLICT (id) DO UPDATE SET
        email = EXCLUDED.email,
        role = EXCLUDED.role,
        full_name = EXCLUDED.full_name,
        student_id = EXCLUDED.student_id,
        ic_number = EXCLUDED.ic_number;
    RETURN new;
EXCEPTION WHEN OTHERS THEN
    RAISE WARNING 'Error in handle_new_user: %', SQLERRM;
    RETURN new;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
CREATE TRIGGER on_auth_user_created
    AFTER INSERT ON auth.users
    FOR EACH ROW EXECUTE FUNCTION public.handle_new_user();

-- 获取当前用户资料
CREATE OR REPLACE FUNCTION get_my_profile()
RETURNS TABLE(
  id UUID, email TEXT, role TEXT, full_name TEXT, avatar_url TEXT,
  student_id TEXT, ic_number TEXT, updated_at TIMESTAMP WITH TIME ZONE
) AS $$
BEGIN
  RETURN QUERY SELECT p.id, p.email, p.role, p.full_name, p.avatar_url, 
    p.student_id, p.ic_number, p.updated_at
  FROM public.profiles p WHERE p.id = auth.uid();
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- 更新头像
CREATE OR REPLACE FUNCTION update_avatar(p_avatar_url TEXT)
RETURNS void AS $$
BEGIN
  UPDATE public.profiles SET avatar_url = p_avatar_url, updated_at = NOW()
  WHERE id = auth.uid();
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;


-- -----------------------------------------------------------------------------
-- 2. ANNOUNCEMENTS (公告)
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
DROP POLICY IF EXISTS "Authenticated can manage announcements" ON public.announcements;

CREATE POLICY "Everyone can read announcements" ON public.announcements FOR SELECT USING (true);
CREATE POLICY "Authenticated can manage announcements" ON public.announcements FOR ALL USING (auth.role() = 'authenticated');


-- -----------------------------------------------------------------------------
-- 3. BUSES (巴士车队管理)
-- -----------------------------------------------------------------------------

DROP TABLE IF EXISTS public.buses CASCADE;

CREATE TABLE public.buses (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    bus_name TEXT NOT NULL,
    plate_number TEXT NOT NULL,
    capacity INTEGER DEFAULT 40,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

ALTER TABLE public.buses ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS "Everyone can read buses" ON public.buses;
DROP POLICY IF EXISTS "Authenticated can manage buses" ON public.buses;

CREATE POLICY "Everyone can read buses" ON public.buses FOR SELECT USING (true);
CREATE POLICY "Authenticated can manage buses" ON public.buses FOR ALL USING (auth.role() = 'authenticated');

GRANT ALL ON TABLE public.buses TO postgres;
GRANT ALL ON TABLE public.buses TO service_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public.buses TO authenticated;
GRANT SELECT ON TABLE public.buses TO anon;

-- 插入示例巴士
INSERT INTO public.buses (bus_name, plate_number, capacity) VALUES
('Bus A', 'QSK 1234', 40),
('Bus B', 'QSK 5678', 35)
ON CONFLICT DO NOTHING;


-- -----------------------------------------------------------------------------
-- 4. BUS_LOCATIONS (巴士实时位置与司机状态)
-- -----------------------------------------------------------------------------

DROP TABLE IF EXISTS public.bus_locations CASCADE;

CREATE TABLE public.bus_locations (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    bus_id UUID REFERENCES public.buses(id) ON DELETE CASCADE,
    driver_id UUID REFERENCES auth.users(id) ON DELETE SET NULL,
    latitude DOUBLE PRECISION,
    longitude DOUBLE PRECISION,
    status TEXT DEFAULT 'Offline', -- 'Driving', 'Resting', 'Offline'
    route_name TEXT, -- Current route: 'Route A' or 'Route B'
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL
);

ALTER TABLE public.bus_locations ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS "Everyone can read bus locations" ON public.bus_locations;
DROP POLICY IF EXISTS "Drivers can update their location" ON public.bus_locations;

CREATE POLICY "Everyone can read bus locations" ON public.bus_locations FOR SELECT USING (true);
CREATE POLICY "Drivers can update their location" ON public.bus_locations FOR ALL USING (auth.uid() = driver_id);
CREATE POLICY "Authenticated can manage bus locations" ON public.bus_locations FOR ALL USING (auth.role() = 'authenticated');

GRANT ALL ON TABLE public.bus_locations TO postgres;
GRANT ALL ON TABLE public.bus_locations TO service_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public.bus_locations TO authenticated;
GRANT SELECT ON TABLE public.bus_locations TO anon;


-- -----------------------------------------------------------------------------
-- 4. SCHEDULES (班次时刻表)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS public.schedules (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    route_name TEXT NOT NULL,
    departure_time TIME NOT NULL,
    day_type TEXT DEFAULT 'Daily',
    status TEXT DEFAULT 'On Time'
);

-- 确保 status 列存在
ALTER TABLE public.schedules ADD COLUMN IF NOT EXISTS status TEXT DEFAULT 'On Time';

ALTER TABLE public.schedules ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS "Everyone can read schedules" ON public.schedules;
DROP POLICY IF EXISTS "Authenticated can manage schedules" ON public.schedules;

CREATE POLICY "Everyone can read schedules" ON public.schedules FOR SELECT USING (true);
CREATE POLICY "Authenticated can manage schedules" ON public.schedules FOR ALL USING (auth.role() = 'authenticated');

-- 获取带状态的班次（服务器端用马来西亚时间计算是否关闭）
DROP FUNCTION IF EXISTS get_schedules_with_status();
CREATE OR REPLACE FUNCTION get_schedules_with_status()
RETURNS TABLE(
    id UUID, route_name TEXT, departure_time TIME, day_type TEXT, status TEXT, is_closed BOOLEAN
) AS $$
DECLARE
    malaysia_now TIME;
BEGIN
    malaysia_now := (NOW() AT TIME ZONE 'Asia/Kuala_Lumpur')::TIME;
    RETURN QUERY
    SELECT 
        s.id, s.route_name, s.departure_time, s.day_type, s.status,
        (malaysia_now > (s.departure_time - INTERVAL '10 minutes')) AS is_closed
    FROM public.schedules s
    ORDER BY s.departure_time ASC;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;


-- -----------------------------------------------------------------------------
-- 5. BUS LOCATIONS (巴士实时位置)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS public.bus_locations (
    driver_id UUID REFERENCES auth.users(id) PRIMARY KEY,
    route_name TEXT,
    latitude DOUBLE PRECISION,
    longitude DOUBLE PRECISION,
    status TEXT,
    last_updated TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now())
);

ALTER TABLE public.bus_locations ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS "Everyone can see buses" ON public.bus_locations;
DROP POLICY IF EXISTS "Drivers can update own location" ON public.bus_locations;

CREATE POLICY "Everyone can see buses" ON public.bus_locations FOR SELECT USING (true);
CREATE POLICY "Drivers can update own location" ON public.bus_locations FOR ALL USING (auth.uid() = driver_id);


-- -----------------------------------------------------------------------------
-- 6. BOOKINGS (预订)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS public.bookings (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    schedule_id UUID REFERENCES public.schedules(id) ON DELETE CASCADE,
    student_id UUID REFERENCES public.profiles(id) ON DELETE CASCADE,
    booking_date DATE DEFAULT CURRENT_DATE,
    status TEXT DEFAULT 'Booked',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now()) NOT NULL,
    UNIQUE(schedule_id, student_id, booking_date)
);

ALTER TABLE public.bookings ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS "Students can manage own bookings" ON public.bookings;
DROP POLICY IF EXISTS "Drivers and Admins can view bookings" ON public.bookings;
DROP POLICY IF EXISTS "All authenticated can manage bookings" ON public.bookings;

-- 简化策略：认证用户可以管理预订
CREATE POLICY "All authenticated can manage bookings" ON public.bookings
    FOR ALL USING (auth.role() = 'authenticated');


-- -----------------------------------------------------------------------------
-- 7. TRIP PASSENGERS (乘客记录)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS public.trip_passengers (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    trip_id TEXT, 
    student_id UUID REFERENCES public.profiles(id),
    boarded_at TIMESTAMP WITH TIME ZONE DEFAULT timezone('utc'::text, now())
);

ALTER TABLE public.trip_passengers ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS "Students can board" ON public.trip_passengers;
DROP POLICY IF EXISTS "Everyone can read passengers" ON public.trip_passengers;

CREATE POLICY "Students can board" ON public.trip_passengers FOR INSERT WITH CHECK (auth.uid() = student_id);
CREATE POLICY "Everyone can read passengers" ON public.trip_passengers FOR SELECT USING (true);


-- -----------------------------------------------------------------------------
-- 8. STORAGE BUCKETS (头像存储)
-- -----------------------------------------------------------------------------

INSERT INTO storage.buckets (id, name, public) 
VALUES ('avatars', 'avatars', true)
ON CONFLICT (id) DO NOTHING;

DROP POLICY IF EXISTS "Avatar images are publicly accessible" ON storage.objects;
DROP POLICY IF EXISTS "Users can upload their own avatar" ON storage.objects;
DROP POLICY IF EXISTS "Users can update their own avatar" ON storage.objects;
DROP POLICY IF EXISTS "Users can delete their own avatar" ON storage.objects;

CREATE POLICY "Avatar images are publicly accessible" ON storage.objects
  FOR SELECT USING (bucket_id = 'avatars');

CREATE POLICY "Users can upload their own avatar" ON storage.objects
  FOR INSERT WITH CHECK (bucket_id = 'avatars' AND auth.role() = 'authenticated');

CREATE POLICY "Users can update their own avatar" ON storage.objects
  FOR UPDATE USING (bucket_id = 'avatars' AND auth.role() = 'authenticated');

CREATE POLICY "Users can delete their own avatar" ON storage.objects
  FOR DELETE USING (bucket_id = 'avatars' AND auth.role() = 'authenticated');


-- -----------------------------------------------------------------------------
-- 9. 刷新 Schema Cache
-- -----------------------------------------------------------------------------

NOTIFY pgrst, 'reload schema';


-- =============================================================================
-- 完成！所有表和函数已创建。
-- =============================================================================

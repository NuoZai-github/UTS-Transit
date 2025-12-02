-- Add 'status' column to 'active_trips' table
alter table public.active_trips 
add column if not exists status text default 'Stopped';

-- Update the policy to allow updates to this column (already covered by existing policy, but good to verify)
-- Existing policy: create policy "Enable update for users based on user_id" ... using (auth.uid() = driver_id);
-- This covers all columns.

-- Optional: Create a function to auto-delete old trips if they are "Stopped" for too long?
-- For now, we'll just rely on the driver explicitly setting status.

-- 1. Create a 'profiles' table to store user data safely
-- This is often required if you have a trigger that tries to write to it.
create table if not exists public.profiles (
  id uuid references auth.users on delete cascade not null primary key,
  email text,
  role text,
  created_at timestamp with time zone default timezone('utc'::text, now()) not null
);

-- 2. Enable Row Level Security (RLS)
alter table public.profiles enable row level security;

-- 3. Create Policies for Profiles
create policy "Public profiles are viewable by everyone." on public.profiles
  for select using (true);

create policy "Users can insert their own profile." on public.profiles
  for insert with check (auth.uid() = id);

create policy "Users can update own profile." on public.profiles
  for update using (auth.uid() = id);

-- 4. Create/Replace the function that handles new user registration
-- This function will run every time a user signs up.
create or replace function public.handle_new_user() 
returns trigger as $$
begin
  insert into public.profiles (id, email, role)
  values (
    new.id, 
    new.email, 
    COALESCE(new.raw_user_meta_data->>'role', 'student') -- Default to 'student' if null
  );
  return new;
end;
$$ language plpgsql security definer;

-- 5. Recreate the Trigger
-- We drop it first to ensure we don't have duplicates or old broken versions.
drop trigger if exists on_auth_user_created on auth.users;

create trigger on_auth_user_created
  after insert on auth.users
  for each row execute procedure public.handle_new_user();


import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://dxinlpyicohuegachjdp.supabase.co'
const supabaseKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImR4aW5scHlpY29odWVnYWNoamRwIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjQwMzM5NjQsImV4cCI6MjA3OTYwOTk2NH0.sloSuXDB0Wtoumucox0Uc5NV2VhMmcGyHdcGf2gYgnc'

export const supabase = createClient(supabaseUrl, supabaseKey)

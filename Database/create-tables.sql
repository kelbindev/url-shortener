-- URL Shortener Database Schema
-- PostgreSQL migration script

-- Create urlshortener_shorturls table
CREATE TABLE IF NOT EXISTS urlshortener_shorturls (
    id SERIAL PRIMARY KEY,
    url VARCHAR(2048) NOT NULL
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_urlshortener_shorturls_id ON urlshortener_shorturls(id);

-- Add comments for documentation
COMMENT ON TABLE urlshortener_shorturls IS 'Stores shortened URLs with their original long URLs';
COMMENT ON COLUMN urlshortener_shorturls.id IS 'Unique identifier for the short URL';
COMMENT ON COLUMN urlshortener_shorturls.url IS 'The original long URL that this short URL redirects to';

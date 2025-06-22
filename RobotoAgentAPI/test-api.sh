#!/bin/bash

# Movie Chat API Test Script
# Make sure the API is running on https://localhost:5001 before running this script

API_URL="https://localhost:5001/api/chat"

echo "========================================="
echo "Testing Movie Chat API"
echo "========================================="
echo ""

# Test 1: Basic movie search
echo "Test 1: Basic movie search for 'The Matrix'"
echo "Request:"
echo "POST $API_URL"
echo '{"message": "Find movies similar to The Matrix"}'
echo ""
echo "Response:"
curl -X POST $API_URL \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{"message": "Find movies similar to The Matrix"}' \
  -k -s | jq '.'
echo ""
echo "========================================="
echo ""

# Test 2: Movie recommendation request
echo "Test 2: Movie recommendation request"
echo "Request:"
echo "POST $API_URL"
echo '{"message": "Recommend some action movies from the 90s"}'
echo ""
echo "Response:"
curl -X POST $API_URL \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{"message": "Recommend some action movies from the 90s"}' \
  -k -s | jq '.'
echo ""
echo "========================================="
echo ""

# Test 3: Specific movie search
echo "Test 3: Specific movie search"
echo "Request:"
echo "POST $API_URL"
echo '{"message": "Tell me about Inception"}'
echo ""
echo "Response:"
curl -X POST $API_URL \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{"message": "Tell me about Inception"}' \
  -k -s | jq '.'
echo ""
echo "========================================="
echo ""

# Test 4: Genre-based search
echo "Test 4: Genre-based search"
echo "Request:"
echo "POST $API_URL"
echo '{"message": "Show me some horror movies"}'
echo ""
echo "Response:"
curl -X POST $API_URL \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{"message": "Show me some horror movies"}' \
  -k -s | jq '.'
echo ""
echo "========================================="
echo ""

# Test 5: Non-movie query (should return empty array)
echo "Test 5: Non-movie query (should return empty array)"
echo "Request:"
echo "POST $API_URL"
echo '{"message": "What is the weather today?"}'
echo ""
echo "Response:"
curl -X POST $API_URL \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{"message": "What is the weather today?"}' \
  -k -s | jq '.'
echo ""
echo "========================================="
echo ""

# Test 6: Empty message (should return error)
echo "Test 6: Empty message (should return error)"
echo "Request:"
echo "POST $API_URL"
echo '{"message": ""}'
echo ""
echo "Response:"
curl -X POST $API_URL \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{"message": ""}' \
  -k -s | jq '.'
echo ""
echo "========================================="
echo ""

# Test 7: Rate limit headers check
echo "Test 7: Check rate limit headers"
echo "Request:"
echo "POST $API_URL"
echo '{"message": "Find movies like Star Wars"}'
echo ""
echo "Response with headers:"
curl -X POST $API_URL \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{"message": "Find movies like Star Wars"}' \
  -k -s -i | head -20
echo ""
echo "========================================="
echo ""

echo "API Testing Complete!"
echo ""
echo "Note: If you see SSL certificate errors, the -k flag ignores them for testing."
echo "In production, you should use proper SSL certificates." 
#!/bin/sh
# Inject runtime API URL into index.html so the same image works in any env.
API_URL="${API_URL:-http://localhost:5080/api}"
SNIPPET="<script>window.__API_URL__='${API_URL}';</script>"
INDEX="/usr/share/nginx/html/index.html"
if ! grep -q "window.__API_URL__" "$INDEX"; then
  sed -i "s|</head>|${SNIPPET}</head>|" "$INDEX"
fi

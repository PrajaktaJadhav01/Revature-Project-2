FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

FROM nginx:1.27-alpine
WORKDIR /usr/share/nginx/html
RUN rm -rf ./*
COPY --from=build /app/dist ./

RUN /bin/sh -c 'cat > /etc/nginx/conf.d/default.conf <<EOF\nserver {\n  listen 80;\n  server_name _;\n  root /usr/share/nginx/html;\n  index index.html;\n\n  location / {\n    try_files $uri /index.html;\n  }\n\n  location /api/ {\n    proxy_pass http://customer-api:5000/api/;\n    proxy_set_header Host $host;\n  }\n}\nEOF'

EXPOSE 80

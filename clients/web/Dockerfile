# ─────────────────────────────────────────────────────────────────────────────
# Stage 1 — build
# VITE_* variables are baked in at build time (browser-side env).
# Pass them via docker-compose build args or `docker build --build-arg`.
# ─────────────────────────────────────────────────────────────────────────────
FROM node:22-alpine AS build
WORKDIR /app

# Layer-cached install
COPY package*.json ./
RUN npm ci

# Build args → Vite env vars
ARG VITE_API_BASE_URL
ARG VITE_AUTH0_DOMAIN
ARG VITE_AUTH0_CLIENT_ID
ARG VITE_AUTH0_AUDIENCE

ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_AUTH0_DOMAIN=$VITE_AUTH0_DOMAIN
ENV VITE_AUTH0_CLIENT_ID=$VITE_AUTH0_CLIENT_ID
ENV VITE_AUTH0_AUDIENCE=$VITE_AUTH0_AUDIENCE

COPY . .
RUN npm run build

# ─────────────────────────────────────────────────────────────────────────────
# Stage 2 — serve with nginx
# ─────────────────────────────────────────────────────────────────────────────
FROM nginx:alpine AS runtime

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]

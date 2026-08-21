FROM node:22-alpine AS build
WORKDIR /src

COPY frontend/packcontrol-web/package.json frontend/packcontrol-web/package-lock.json ./
RUN npm ci

COPY frontend/packcontrol-web/ .
RUN npm run build

FROM nginx:1.27-alpine AS runtime
COPY deploy/nginx/packcontrol.conf /etc/nginx/conf.d/default.conf
COPY --from=build /src/dist/packcontrol-web/browser /usr/share/nginx/html

EXPOSE 80

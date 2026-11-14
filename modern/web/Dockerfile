FROM node:24-alpine AS build
WORKDIR /workspace
COPY cqrs-es-todos-ui/package*.json ./
RUN npm install --no-audit --no-fund
COPY cqrs-es-todos-ui/ ./
RUN npm run build

FROM nginx:1.29-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /workspace/dist/cqrs-es-todos-ui/browser /usr/share/nginx/html
EXPOSE 80

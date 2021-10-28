FROM nginx:1.7.9
WORKDIR /app
EXPOSE 80

COPY nginx.conf /etc/nginx/
COPY ./dist .
ENTRYPOINT nginx -g "daemon off;"

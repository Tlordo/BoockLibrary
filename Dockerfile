FROM node:22-alpine
WORKDIR /app
ENV NODE_ENV=production
COPY src/BookLibrary.Js/package*.json ./
RUN npm ci --omit=dev
COPY src/BookLibrary.Js/server.js ./
RUN mkdir -p /data && chown node:node /data
USER node
ENV PORT=3000 DB_PATH=/data/booklibrary.db
EXPOSE 3000
CMD ["node", "server.js"]

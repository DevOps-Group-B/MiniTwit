# MiniTwit

## Docker setup

```bash
cd itu-minitwit/
docker build -t minitwit/webserver .
docker run -p 5273:5273 minitwit/webserver
```
# MiniTwit

## Docker

### Setup

```bash
cd itu-minitwit/
docker build -t minitwit/webserver .
docker run -p 5273:5273 --name minitwit minitwit/webserver
```

### Start

```bash
docker start minitwit
```

### Stop

```bash
docker stop minitwit
```
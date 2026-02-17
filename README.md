# MiniTwit

## Docker

### Local Setup

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

### Deploy Setup
Firstly:
```bash
git clone https://github.com/DevOps-Group-B/MiniTwit.git
```

Then setup Environment for the following:
- SSH_KEY_NAME
- SSH_KEY_PATH
- DIGITAL_OCEAN_TOKEN

Lastly run the command:
```bash
vagrant up
```

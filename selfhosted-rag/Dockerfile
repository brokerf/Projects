FROM python:3.13
WORKDIR /app
RUN apt-get update && apt-get install -y build-essential curl 
RUN pip install --no-cache-dir ollama
COPY script.py .
COPY facts.txt .

VOLUME /app/data

CMD ["python", "script.py"]
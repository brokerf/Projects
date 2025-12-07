FROM python:3.10-slim
RUN pip install mlflow==2.9 scikit-learn pandas
COPY model.pkl /app/model.pkl
WORKDIR /app
CMD ["mlflow", "model", "serve", "-m", "/app/model.pkl", "--port", "5000"]
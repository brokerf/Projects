FROM    python:3.10-slim
RUN pip install mlflow==3.7.0 scikit-learn pandas joblib
COPY /mlruns/0/models/m-43c9f15be5af40dc82ecbd0d6039e167/artifacts /app/model
WORKDIR /app
EXPOSE 5000
CMD ["mlflow", "models", "serve", "-m", "/app/model", "--host", "0.0.0.0", "--port", "5000", "--no-conda", "--env-manager", "local"]
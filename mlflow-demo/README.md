A quick demonstration of fitting a 
RandomForestClassifer ML Model
using the iris set and using the docker
container to deploy and utilize it
on any machine

## Training
python train.py

## Serving
docker build -t mlflow-demo .
docker run -p 5000:5000 mlflow-demo

## Test request
curl -X POST http://localhost:5000/invocations --data @request.json

## Request generation
python gen_values_model.py

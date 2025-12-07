import mlflow
import mlflow.sklearn
from sklearn.datasets import load_iris
from sklearn.ensemble import RandomForestClassifier
import joblib

iris = load_iris()
model = RandomForestClassifier()
model.fit(iris.data, iris.target)

joblib.dump(model, "model.pkl")

mlflow.sklearn.log_model(model, "model1")
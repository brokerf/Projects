import ollama
import os
import time

EMBEDDING_MODEL = "hf.co/CompendiumLabs/bge-base-en-v1.5-gguf"
LANGUAGE_MODEL = "hf.co/bartowski/Llama-3.2-1B-Instruct-GGUF"
OLLAMA_HOST = os.getenv('OLLAMA_HOST', 'http://localhost:11434')

print(f'Connecting to Ollama at: {OLLAMA_HOST}')

max_retries = 10
retry_delay = 5

for i in range(max_retries):
    try:
        client = ollama.Client(host=OLLAMA_HOST)
        # Test connection
        client.list()
        print("Successfully connected to Ollama!")
        break
    except Exception as e:
        if i < max_retries - 1:
            print(f"Failed to connect to Ollama (attempt {i+1}/{max_retries}): {e}")
            print(f"Retrying in {retry_delay} seconds...")
            time.sleep(retry_delay)
        else:
            print(f"Failed to connect to Ollama after {max_retries} attempts: {e}")
            print("Please ensure Ollama is running and models are downloaded.")
            exit(1)

VECTOR_DB = []

def add_chunk_to_database(chunk):
    embedding = ollama.embed(model=EMBEDDING_MODEL, input=chunk)["embeddings"][0]
    VECTOR_DB.append((chunk, embedding))

dataset = []
with open("facts.txt", mode="r", encoding="utf8") as file:
    dataset = file.readlines()
    print(f"Loaded {len(dataset)} entries")

for i, chunk in enumerate(dataset):
    add_chunk_to_database(chunk)
    print(f"Added {i+1}/{len(dataset)} to the database")

def cosine_similarity(a, b):
    dot_product = sum([x * y for x, y in zip(a, b)])
    norm_a = sum([x ** 2 for x in a]) ** 0.5
    norm_b = sum([y ** 2 for y in b]) ** 0.5
    return dot_product / (norm_a * norm_b)

def retrieve(query, top_n=3):
    query_embedding = ollama.embed(model=EMBEDDING_MODEL, input=query)["embeddings"][0]

    similarities = []
    for chunk, embedding in VECTOR_DB:
        similarity = cosine_similarity(query_embedding, embedding)
        similarities.append((chunk, similarity))
    similarities.sort(key=lambda x:x[1], reverse=True)
    return similarities[:top_n]
    
input_query = input("Ask me anything: ")
retrieved_knowledge = retrieve(input_query)

print("Retrieved Knowledge:")
for chunk, similarity in retrieved_knowledge:
    print(f" - (similarity: {similarity:.2f}) {chunk}")

instruction_prompt = f'''You are a helpful chatbot.
Use only the following pieces of context to answer the question. Don't make up any new information:
{'\n'.join([f'- {chunk}' for chunk, similarity in retrieved_knowledge])}
'''

stream = ollama.chat(
    model=LANGUAGE_MODEL,
    messages=[
        {"role": "system", "content": instruction_prompt},
        {"role": "user", "content": input_query},
    ],
    stream=True
)

print("Chatbot respose:")
for chunk in stream:
    print(chunk["message"]["content"], end="", flush=True)
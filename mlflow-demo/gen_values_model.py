import random as rng
import string

# Generate 1000 random values to train the model
with open("request.json", "w") as file:
    file.write('{\n\t"dataframe_records": [\n\t\t{')
    content = ""
    
    for i in range(0, 4):
        
        #generate random 4char string and random float, up to 1 decimal
        value = round(rng.uniform(0.0, 8.0), 1)
        name = ''.join(rng.choice(string.ascii_uppercase + string.digits) for _ in range(4))
        content = content + (f'\n\t\t\t"{name}": {value},')
    
    #remove the last comma for correct formatting
    content = content[0:-1]
    
    file.write(content + "\n")
    file.write("\t\t}\n")
    file.write("\t]\n")
    file.write("}")
file.close()
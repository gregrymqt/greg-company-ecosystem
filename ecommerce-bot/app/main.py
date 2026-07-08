import logging
from dotenv import load_dotenv
from app.services.message_consumer import start_consuming

load_dotenv()
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')

if __name__ == "__main__":
    start_consuming()
package config

import(
	"os"
	"github.com/joho/godotenv"
	"fmt"
)

type AppConfig struct {
	MongoDBURI string
	RedisDBURL string
	RabbitMqURL string
	SendGridApiKey string 
	SendGridFromEmail string 
	SendGridFromName string
	USEREDIS string
}

func LoadEnv() *AppConfig {
	err := godotenv.Load();
	if err != nil {
		fmt.Println(" Erro ao carregar as variáveis de ambiente!!")
	}

	envStrings := os.LookupEnv()
}
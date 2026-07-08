package configenv

import (
	"os"
)

type ConfigEnv struct {
	RabbitMqURL             string
	SupabaseS3URL           string
	SupabaseAccessKeyId     string
	SupabaseSecretAccessKey string
}

func LoadConfig() *ConfigEnv {
	return &ConfigEnv{
		RabbitMqURL:             getEnv("RABBITMQ_URL", "amqp://guest:guest@localhost:5672/"),
		SupabaseS3URL:           getEnv("SUPABASE_S3_URL", "http://localhost:54321/storage/v1/s3"),
		SupabaseAccessKeyId:     getEnv("SUPABASE_ACCESS_KEY_ID", "default_access_key"),
		SupabaseSecretAccessKey: getEnv("SUPABASE_SECRET_ACCESS_KEY", "default_secret_key"),
	}
}

func getEnv(key, defaultValue string) string {
	if value, exists := os.LookupEnv(key); exists {
		return value
	}
	return defaultValue
}

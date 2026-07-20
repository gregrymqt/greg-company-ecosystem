package configenv

import (
	"os"
	"strings"
)

type ConfigEnv struct {
	RabbitMqURL             string
	SupabaseS3URL           string
	SupabaseAccessKeyId     string
	SupabaseSecretAccessKey string
	SendGridApiKey          string
	SendGridFromEmail       string
	SendGridFromName        string
}

func loadDotEnv() {
	files := []string{".env", "../.env"}
	for _, f := range files {
		data, err := os.ReadFile(f)
		if err != nil {
			continue
		}
		lines := strings.Split(string(data), "\n")
		for _, line := range lines {
			line = strings.TrimSpace(line)
			if line == "" || strings.HasPrefix(line, "#") {
				continue
			}
			parts := strings.SplitN(line, "=", 2)
			if len(parts) == 2 {
				k := strings.TrimSpace(parts[0])
				v := strings.TrimSpace(parts[1])
				v = strings.Trim(v, `"'`)
				if _, exists := os.LookupEnv(k); !exists {
					_ = os.Setenv(k, v)
				}
			}
		}
	}
}

func LoadConfig() *ConfigEnv {
	loadDotEnv()
	return &ConfigEnv{
		RabbitMqURL:             getEnv("RABBITMQ_URL", "amqp://guest:guest@localhost:5672/", "RABBITMQ_URI"),
		SupabaseS3URL:           getEnv("SUPABASE_S3_URL", "http://localhost:54321/storage/v1/s3", "EndPoint_S3", "ENDPOINT_S3"),
		SupabaseAccessKeyId:     getEnv("SUPABASE_ACCESS_KEY_ID", "default_access_key", "Access_key_ID", "ACCESS_KEY_ID"),
		SupabaseSecretAccessKey: getEnv("SUPABASE_SECRET_ACCESS_KEY", "default_secret_key", "Secret_Access_key", "SECRET_ACCESS_KEY"),
		SendGridApiKey:          getEnv("SENDGRID_API_KEY", "", "SendGrid__ApiKey"),
		SendGridFromEmail:       getEnv("SENDGRID_FROM_EMAIL", "noreply@gregcompany.com", "SendGrid__FromEmail"),
		SendGridFromName:        getEnv("SENDGRID_FROM_NAME", "Greg Company", "SendGrid__FromName"),
	}
}

func getEnv(key, defaultValue string, aliases ...string) string {
	if value, exists := os.LookupEnv(key); exists && value != "" {
		return value
	}
	for _, alias := range aliases {
		if value, exists := os.LookupEnv(alias); exists && value != "" {
			return value
		}
	}
	return defaultValue
}

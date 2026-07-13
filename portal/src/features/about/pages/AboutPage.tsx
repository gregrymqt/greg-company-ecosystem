import React from "react";
import styles from "./AboutPage.module.scss";

export const AboutPage: React.FC = () => {
  return (
    <main className={styles.pageContainer}>
      {/* Hero Section */}
      <section className={styles.heroSection}>
        <div className={styles.container}>
          <h1 className={styles.title}>Sobre o Ecossistema Greg Company</h1>
          <p className={styles.description}>
            A Greg Company é um ecossistema completo e distribuído para gestão de cursos online e inteligência de negócios. Construído sob uma arquitetura de Monorepo, a plataforma resolve o desafio de gerenciar pagamentos, assinaturas e transcodificação pesada de vídeos sem comprometer a performance. Utilizamos um backend robusto em .NET 8 com Clean Architecture e Transactional Outbox, delegando o processamento assíncrono para microsserviços em Go via RabbitMQ, garantindo resiliência e alta disponibilidade.
          </p>
        </div>
      </section>

      {/* Developer Section */}
      <section className={styles.devSection}>
        <div className={styles.container}>
          <h2 className={styles.sectionTitle}>Engenharia / Desenvolvedor</h2>
          
          <div className={styles.devCard}>
            <div className={styles.devInfo}>
              <h3 className={styles.devName}>Lucas Vicente de Souza</h3>
              <p className={styles.devRole}>Full-Stack Developer & Software Engineering Student (FATEC Praia Grande)</p>
              <p className={styles.devBio}>
                Estudante de Desenvolvimento de Software Multiplataforma, apaixonado por construir sistemas distribuídos, resilientes e escaláveis. Focado no ecossistema .NET (C#), Go (Golang) para alta concorrência e React (TypeScript) no front-end. Possui experiência prática com Clean Architecture, mensageria via RabbitMQ e estratégias de cache com Redis para proteção de bancos de dados como PostgreSQL e MongoDB.
              </p>
              
              <div className={styles.socialLinks}>
                <a href="https://www.linkedin.com/in/lucas-vicente-dev/" target="_blank" rel="noopener noreferrer" className={styles.socialLink}>
                  <i className="fab fa-linkedin"></i> LinkedIn
                </a>
                <a href="https://github.com/gregrymqt" target="_blank" rel="noopener noreferrer" className={styles.socialLink}>
                  <i className="fab fa-github"></i> GitHub
                </a>
              </div>
            </div>
          </div>
        </div>
      </section>
    </main>
  );
};

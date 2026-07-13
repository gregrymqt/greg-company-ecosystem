import React, { useState } from 'react';
import styles from '../pages/Home.module.scss';
import type { ServiceDto } from '../types/home.types';

interface ServicesProps {
  data: ServiceDto[];
}

export const Services: React.FC<ServicesProps> = ({ data }) => {
  const [activeTab, setActiveTab] = useState<'student' | 'merchant'>('student');

  const filteredData = data.filter(item => 
    item.audience === activeTab || item.audience === 'universal' || !item.audience
  );

  return (
    <section className={styles.servicesSection} id="services">
      <div className="container">
        <h2 className="section-title">Nossas Soluções</h2>
        
        {/* Tabs de Seleção */}
        <div className={styles.tabsContainer}>
          <button 
            onClick={() => setActiveTab('student')}
            className={`${styles.tabButton} ${styles.studentTab} ${activeTab === 'student' ? styles.active : ''}`}
          >
            Plataforma de Cursos
          </button>
          <button 
            onClick={() => setActiveTab('merchant')}
            className={`${styles.tabButton} ${styles.merchantTab} ${activeTab === 'merchant' ? styles.active : ''}`}
          >
            Automações de IA
          </button>
        </div>

        <div className={styles.grid}>
          {filteredData.map((item) => (
            <div key={item.id} className={styles.serviceCard}>
              <i className={item.iconClass}></i>
              <h3>{item.title}</h3>
              <p>{item.description}</p>
              <a href={item.actionUrl} className="btn btn-secondary">
                {item.actionText}
              </a>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
};

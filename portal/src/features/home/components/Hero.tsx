import React from 'react';
import styles from '../../../pages/Home/styles/Home.module.scss';
import { useScrollOpacity } from '@/features/home/hooks/useScrollOpacity';
import type { HeroSlideDto } from '@/features/home/types/home.types';

interface HeroProps {
  slides: HeroSlideDto[];
}

export const Hero: React.FC<HeroProps> = ({ slides }) => {
  const opacity = useScrollOpacity(80); // 80vh height

  if (!slides || slides.length === 0) return null;

  // Busca slides por público ou usa fallback seguro
  const studentSlide = slides.find(s => s.audience === 'student') || slides[0];
  const merchantSlide = slides.find(s => s.audience === 'merchant') || slides[1] || slides[0];

  return (
    <section className={styles.heroSection} style={{ opacity }}>
      {/* Lado do Estudante */}
      <div 
        className={`${styles.heroColumn} ${styles.studentSide}`}
        style={{ backgroundImage: `url(${studentSlide.imageUrl})` }}
      >
        <div className={styles.slideContent}>
          <h1>{studentSlide.title}</h1>
          <p>{studentSlide.subtitle}</p>
          <a href={studentSlide.actionUrl} className={styles.btnStudent}>
            {studentSlide.actionText}
          </a>
        </div>
      </div>

      {/* Lado do Lojista */}
      <div 
        className={`${styles.heroColumn} ${styles.merchantSide}`}
        style={{ backgroundImage: `url(${merchantSlide.imageUrl})` }}
      >
        <div className={styles.slideContent}>
          <h1>{merchantSlide.title}</h1>
          <p>{merchantSlide.subtitle}</p>
          <a href={merchantSlide.actionUrl} className={styles.btnMerchant}>
            {merchantSlide.actionText}
          </a>
        </div>
      </div>
    </section>
  );
};

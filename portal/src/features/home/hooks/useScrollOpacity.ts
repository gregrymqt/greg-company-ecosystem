import { useState, useEffect } from 'react';

export const useScrollOpacity = (fadeHeightVh = 80) => {
  const [opacity, setOpacity] = useState(1);

  useEffect(() => {
    const handleScroll = () => {
      const scrollY = window.scrollY;
      const windowHeight = window.innerHeight;
      const fadeHeightPx = (windowHeight * fadeHeightVh) / 100;
      
      const newOpacity = 1 - Math.min(scrollY / fadeHeightPx, 1);
      setOpacity(Math.max(0, newOpacity));
    };

    window.addEventListener('scroll', handleScroll, { passive: true });
    handleScroll(); // init

    return () => window.removeEventListener('scroll', handleScroll);
  }, [fadeHeightVh]);

  return opacity;
};

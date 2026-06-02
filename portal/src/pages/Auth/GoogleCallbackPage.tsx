import { useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from '@/features/auth/hooks/useAuth';
import styles from './GoogleCallbackPage.module.scss';

export const GoogleCallbackPage = () => {
  const navigate = useNavigate();
  const { handleGoogleCallback } = useAuth();

  const processedRef = useRef(false);

  useEffect(() => {
    if (processedRef.current) return;

    const hash = window.location.hash;
    if (!hash) {
      console.error("Token não encontrado na URL de retorno");
      navigate("/login?error=no_token");
      return;
    }
    const params = new URLSearchParams(hash.slice(1));
    const token = params.get("token");

    if (token) {
      processedRef.current = true;
      // Clear the fragment from the URL immediately so the token is not retained
      window.history.replaceState(null, '', window.location.pathname);
      handleGoogleCallback(token);
    } else {
      console.error("Token não encontrado na URL de retorno");
      navigate("/login?error=no_token");
    }
  }, [handleGoogleCallback, navigate]);

  return (
    <div className={styles["google-callback-page"]}>
      <div className={styles["callback-content"]}>
        <h2>Autenticando...</h2>
        <div className={styles["spinner"]}></div>
      </div>  
    </div>
  );
};

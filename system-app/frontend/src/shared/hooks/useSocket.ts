import { useEffect } from 'react';
import { socketService } from '@/shared/services/socket.service';
import type { AnyAppHub } from '@/shared/enums/hub.enums';

export const useSocketListener = <T>(
  hub: AnyAppHub, 
  eventName: string, 
  callback: (data: T) => void
) => {
  useEffect(() => {
    // Garante conexão antes de ouvir (opcional, dependendo da sua estratégia)
    socketService.connect(hub); 

    socketService.on<T>(hub, eventName, callback);

    return () => {
      socketService.off(hub, eventName);
    };
  }, [hub, eventName, callback]);
};
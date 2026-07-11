# 📁 internal/processor/

**Objetivo:** Conter a "inteligência" e as regras de negócio core do Worker.

**O que colocar aqui:**
- Scripts e chamadas ao sistema operacional, como por exemplo encapsulamento de chamadas ao pacote `os/exec` para rodar o **FFmpeg** em transcodificação de vídeos.
- Lógicas de montagem de templates HTML para os emails (integrando SendGrid ou SMTP base).

**Regras:**
- É a camada mais densa de CPU. O processamento deve preferencialmente ocorrer com chamadas seguras (controlando concorrência por *Goroutines* e *WaitGroups* para não esgotar RAM/CPU).

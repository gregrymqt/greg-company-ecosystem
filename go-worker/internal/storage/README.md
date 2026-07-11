# 📁 internal/storage/

**Objetivo:** Interagir com APIs de sistema de arquivos distribuídos ou nuvem.

**O que colocar aqui:**
- O cliente HTTP e os wrappers para o **Supabase Storage** (ou S3).
- Funções para realizar *Upload*, *Download*, ou assinar URLs (*Signed URLs*) dos vídeos transcodificados.

**Regras:**
- Ao processar arquivos grandes, trabalhe com *Streams* (`io.Reader` / `io.Writer`) ao invés de carregar o arquivo inteiro na memória do Go, garantindo baixo consumo de RAM.

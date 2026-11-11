# Gerador do banco bíblico

O aplicativo empacota `Resources\Raw\bible.db` e consulta localmente as três traduções:

- Português: Bíblia Livre (BLIVRE), fevereiro de 2018.
- Inglês: World English Bible Protestant Edition (WEB).
- Espanhol: Reina-Valera 1909.

Para recriar o banco, baixe e extraia os arquivos VPL das fontes oficiais:

- Português: <https://ebible.org/Scriptures/porbr2018_vpl.zip>
- Inglês: <https://ebible.org/Scriptures/engwebp_vpl.zip>
- Espanhol: <https://ebible.org/Scriptures/spaRV1909_vpl.zip>

Execute na raiz do projeto, informando os três arquivos extraídos:

```powershell
dotnet run --project tools\BibleDatabaseBuilder\BibleDatabaseBuilder.csproj -- `
  C:\caminho\porbr2018_vpl.txt `
  C:\caminho\engwebp_vpl.txt `
  C:\caminho\spaRV1909_vpl.txt `
  Resources\Raw\bible.db
```

O gerador valida os 66 livros, cobertura de capítulos compatível entre as traduções, referências duplicadas, Gênesis 1:1, integridade e chaves estrangeiras do SQLite antes de substituir o banco. Versículos sem texto em uma tradução são mantidos como registros vazios para preservar a numeração.

Consulte `Resources\Raw\BIBLE_LICENSE.txt` para atribuições e licenças de cada texto.

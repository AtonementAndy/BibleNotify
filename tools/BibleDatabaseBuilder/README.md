# Gerador do banco bíblico

O aplicativo empacota `Resources\Raw\bible.db`; a leitura em execução é local e não faz chamadas de rede.

Para recriar o banco, baixe o arquivo VPL oficial da Bíblia Livre:

`https://ebible.org/Scriptures/porbr2018_vpl.zip`

Extraia `porbr2018_vpl.txt` e execute na raiz do projeto:

```powershell
dotnet run --project tools\BibleDatabaseBuilder\BibleDatabaseBuilder.csproj -- C:\caminho\porbr2018_vpl.txt Resources\Raw\bible.db
```

O gerador valida os 66 livros, 31.102 versículos, Gênesis 1:1 e a integridade SQLite antes de concluir.
Preserve o aviso de licença em `Resources\Raw\BIBLE_LICENSE.txt` ao redistribuir o app.

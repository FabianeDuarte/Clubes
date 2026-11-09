# Projeto Clubes

Três projetos:

| Projeto | O que é |
|---|---|
| `Clubes.Shared` | Modelos, enums e DTOs usados pelo app e pelo servidor (+ validação de CPF) |
| `Clubes.Server` | ASP.NET Core: **API do app** (JWT) + **painel web** (Razor Pages, cookie) + SQLite |
| `Clubes.App` | App .NET MAUI (código-fonte; o esqueleto Platforms/Resources vem do template) |

> Este código foi escrito sem compilar (sem SDK .NET no ambiente de geração). Pode haver ajustes pequenos
> de compilação ou de versão de pacote na primeira abertura.

## Montando a solução (Windows / Visual Studio, .NET 10)
1. Descompacte e, na pasta raiz, rode `./criar-solucao.ps1` (cria o .sln, gera o esqueleto MAUI e liga tudo).
2. Abra `Clubes.sln`.

## Rodando
1. Defina `Clubes.Server` como projeto de inicialização e rode. Ele sobe em `http://0.0.0.0:5080`
   e cria o banco `clubes.db` com dados de demonstração.
2. Painel web: `http://localhost:5080` (home pública com agenda do salão e eventos abertos) → **Entrar**.
3. App: rode `Clubes.App`. Android emulador acessa o PC em `10.0.2.2:5080`; aparelho real: ajuste
   `ApiConfig.BaseUrl` (Services/ClubeServiceHttp.cs) para o IP do PC e libere a porta 5080 no firewall.
4. Android e HTTP: no `Platforms/Android/AndroidManifest.xml`, na tag `<application>`, adicione
   `android:usesCleartextTraffic="true"` (somente desenvolvimento; em produção use HTTPS).

## Contas de demonstração (TROQUE antes de usar de verdade)
| Perfil | E-mail | Senha |
|---|---|---|
| Super admin | admin@clubes.local | Admin@123 |
| Presidente (Foz 2) | presidente.foz2@clubes.local | Clube@123 |
| Tesoureiro (Foz 2) | tesoureiro.foz2@clubes.local | Clube@123 |
| Sócio fundador (app) | joao@clubes.local | Socio@123 |

Também troque `Jwt:Key` em `appsettings.json` por uma chave secreta sua.

## Quem faz o quê no painel web
- **Super admin**: tudo, de todos os clubes (cadastra/edita clubes e cria presidente e tesoureiro de cada clube em *Usuários*).
- **Presidente/admin do clube**: aprova/rejeita associados, cadastra e edita associados (cadastro completo),
  cria tesoureiros do próprio clube, eventos, reservas do salão, responde solicitações e reclamações, vê mensalidades.
- **Tesoureiro**: mensalidades (gerar do mês, dar baixa, estornar, filtrar por situação).

## Regras que implementei (confirme se é isso que você quer)
- **Todo autocadastro pelo app entra como Pendente**, inclusive sócio fundador/associado (que votam),
  para ninguém ganhar direito a voto sozinho. O presidente também pode cadastrar já aprovado.
- Pendente consegue entrar no app, mas só vê a conta; mensalidades, reservas, solicitações e dependentes
  exigem aprovação.
- Direito a voto: fundador e associado. Convidado e simpatizante: não.
- **Simpatizante não recebe mensalidade** na geração automática (ajuste em `Regras.GerarMensalidades`).
- Mensalidade: valor mensal por clube, vencimento dia 10; situação = Paga / Pendente / Atrasada.
- Evento "somente associados": só sócio aprovado (fundador, associado ou convidado) e seus dependentes veem.
  Simpatizante, cadastro pendente e visitante veem só os eventos "abertos ao público" (que também aparecem no site).
- **Dependente** pode ter login próprio no app (o titular cria em *Conta*): acesso somente à consulta de eventos.
- Reserva do salão: data/hora de início e fim, bloqueia períodos que se cruzam; sócio pede (fica Pendente),
  presidente aprova. O presidente também bloqueia datas do clube (ex.: feijoada beneficente), que já
  aparecem na agenda pública.

## Ainda não existe
Pagamento online/boleto, votação em assembleia, notificações push, recuperação de senha, tela no app para
editar o próprio perfil, HTTPS/produção, migrações EF (usa `EnsureCreated`: **apague o `clubes.db` antes de rodar
esta versão**, pois os modelos mudaram).

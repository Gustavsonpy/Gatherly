**Engenharia de Software – Católica SC**

---

# Identificação

- **Título do Projeto:**  
  Gatherly.

- **Linha de Projeto (Direction):**  
  Web

- **Autor:**  
  Gustavo Mafra Paluski

- **Data da Proposta:**  
  12/04/2026

- **Versão:**  
  1.0

---

# 1. Visão do Produto e Impacto (O Problema)

## 1.1 Contexto e Problema

O projeto tem como objetivo resolver um problema real relacionado à dificuldade de conexão entre pessoas em eventos presenciais e sociais. Atualmente, se tem visto programações informais, seja praticar algum esporte entre amigos, encontros ou atividades em grupo, serem canceladas por falta de participantes suficientes, causando o cancelamento da atividade.

Além disso, existem pessoas que se mudam para outras cidades ou que possuem poucas amizades, que enfrentam dificuldades em encontrar oportunidades de socialização e de encontros entre pessoas. A ausência de uma plataforma centralizada que facilite a descoberta e participação em eventos locais contribui para esse cenário.

Atualmente esses problemas são resolvidos através de grupos em redes sociais ou em recomendações de amigos, fazendo com que os interessados tenham que estar em diversos grupos e ter instalado vários aplicativos de redes sociais para confirmar por mensagem que irá participar dos encontros e eventos, tendo a possibilidade dessa mensagem se perder, dificultando o trabalho dos organizadores.

Exemplo:

<img src="img_readme/lista_fut_borrado.png" alt="lista_de_confirmados_futebol">

IMAGEM  01 - FOTO DE MENSAGEM COM AS LISTAS DOS CONFIRMADOS PARA UMA PARTIDA DE FUTEBOL.   

Com isso, o projeto não se limita a um desafio técnico, mas busca atender a uma necessidade real, de facilitar a organização, descoberta e participação em eventos, promovendo a conexão entre pessoas nas suas localidades.  

---

## 1.2 Origem da Demanda e Evidências

A fim de validar a relevância do projeto e descobrir as dores latentes, foi conduzida uma pesquisa, via formulário eletrônico, para identificar lacunas na criação de eventos informais entre amigos, colegas e entre pessoas desconhecidas.

A pesquisa continha perguntas sobre como era a dificuldade do entrevistado em criar e participar de eventos, sendo por meios externos ou pessoais, e como um sistema iria ajudar a diminuir ou amenizar esse problema.

Ao todo, 19 (dezenove) pessoas responderam ao formulário. Os resultados, como mostra a imagem abaixo, indicam que as principais dificuldades estão relacionadas à desistência de participantes (47,4%) e à dificuldade de atingir o número mínimo de pessoas necessárias para a realização do evento (31,6%).

<img src="img_readme/spreadsheet_some_problems.png" alt="Maior dificuldade em realizar eventos">

IMAGEM 02 - GRÁFICO MOSTRANDO AS MAIORES DIFICULDADES AO ORGANIZAR EVENTOS 

Além disso, a pesquisa demonstra que existe um possível interesse de uso da plataforma pelos usuários, onde 78,9% dos entrevistados responderam que usariam o novo sistema proposto.

<img src="img_readme/spreadsheet_use.png" alt="Usaria esse sistema">

IMAGEM 03 - GRÁFICO MOSTRANDO COMO SÃO MARCADOS OS EVENTOS ATUALMENTE

Esses dados evidenciam que há uma dor recorrente na organização de eventos informais, especialmente no que diz respeito à previsibilidade de participação e à formação de grupos. Tais dificuldades reforçam a necessidade de uma solução que facilite a descoberta de participantes e aumente o engajamento, validando a proposta do projeto.

## 1.3 Análise de Soluções Existentes (Benchmark)

<h2>Meetup</h2>

O Meetup é uma plataforma voltada à criação e participação em eventos baseados em interesses em comum, organizados principalmente por grupos temáticos. Seu principal objetivo é promover conexões sociais e profissionais por meio de encontros presenciais ou online.

<img src="img_readme/meetup_img.png" alt="Print da plataforma Meetup">

IMAGEM 04 - IMAGEM DA PLATAFORMA "MEETUP"

Link: https://www.meetup.com/pt-BR/

**Público-alvo**:
Pessoas que querem se conectar com outras por meio de interesses em comum

**Funcionalidades principais**: 
- Criação e descoberta de eventos;
- Organização por grupos de interesse;
- Confirmação de presença (RSVP);
- Interação entre usuários.

<h3>Eventbrite</h3>

O Eventbrite é uma plataforma voltada à criação, gestão e comercialização de eventos, oferecendo recursos como venda de ingressos, controle de participantes e ferramentas de divulgação. Seu foco está em eventos estruturados e de maior escala, como shows e conferências. No entanto, apresenta limitações em cenários informais e dinâmicos, especialmente aqueles que exigem rápida organização e participação espontânea.

<img src="img_readme/eventbrite_img.png" alt="Print da plataforma Eventbrite">

IMAGEM 05 - IMAGEM DA PLATAFORMA "Eventbrite"

Link: https://www.eventbrite.com.br/

**Público-alvo**: 
Participantes em busca de eventos e organizadores de eventos que buscam criar, divulgar e monetizar experiências.

**Funcionalidades principais**: 
- Criação e descoberta de eventos;
- Venda e gestão de ingressos;
- Pagamentos integrados (compras direto pelo app);
- Check-in de validação;
- Painel para organizadores.

<h3>Fubles</h3>

O Fubles é uma plataforma voltada à organização de partidas informais de futebol, permitindo a criação de jogos e a participação aberta de jogadores para completar equipes. A solução atende diretamente ao problema de formação de grupos em atividades esportivas, porém é limitada a um nicho específico, não contemplando outros tipos de eventos sociais.

<img src="img_readme/fubles_img.png" alt="Print da plataforma Fubles">

IMAGEM 06 - IMAGEM DA PLATAFORMA "Fubles"

Link: https://www.fubles.com/pt/

**Público-alvo**: 
Pessoas que jogam futebol amador ou profissional, como foco em jogar por lazer

**Funcionalidades principais**: 
- Criação de partidas;
- Entrada em partidas;
- Sistema de avaliação;
- Comunicação

## Comparação
| Solução   | Pontos fortes                                                                 | Limitações                                                                                  |
|-----------|------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------|
| Meetup    | Forte na descoberta de eventos; <br> Organização por grupos de interesse; <br> Facilita networking e socialização; <br> Boa base de usuários. | Foco em eventos planejados; <br> Pouca eficiência para eventos de última hora; <br> Baixa dinamicidade na entrada de participantes; <br> Sistema de reputação limitado. |
| Eventbrite| Criação e gestão robusta de eventos; <br> Sistema completo de venda de ingressos; <br> Ferramentas de divulgação e análise; <br> Alta escalabilidade | Voltado a eventos formais e pagos; <br> Pouca interação social entre participantes; <br> Não atende bem eventos informais; <br> Baixa flexibilidade para eventos espontâneos |
| Fubles    | Entrada aberta de participantes; <br> Resolve o problema de completar vagas; <br> Sistema de avaliação de usuários; <br> Simples e direto | Restrito ao futebol; <br> Não abrange outros tipos de eventos; <br> Pouco foco em socialização geral; <br> Baixa flexibilidade de uso |
| Gatherly  | Foco em eventos informais e espontâneos; <br> Aplicável a diversos tipos de eventos; <br> Sistema de avaliação de usuários | Dependência de uma base inicial de usuários ativa para garantir a criação e participação em eventos; <br> Necessidade de gerar confiança entre usuários; <br> Ainda sem validação em larga escala |

---

### Diferencial do Projeto

O principal diferencial do projeto está na proposta de integrar, em uma única plataforma, a criação de eventos informais, a participação dinâmica de usuários e a conexão social entre pessoas, com foco em situações do cotidiano.

Diferentemente das outras soluções existentes, que são focados em eventos pagos ou de um nicho muito específico, como os que possuem foco apenas em futebol, esse projeto busca que os usuários possam realizar e participar de eventos espontâneos e de baixa formalidade, permitindo criarem atividades de forma rápida e encontrem participantes em tempo real.

Além disso, a plataforma permite a participação de usuários em eventos organizados tanto por conhecidos quanto por desconhecidos, ajudando a reduzir problemas recorrentes, como o cancelamento de atividades devido à falta de participantes suficientes. E, para ajudar na segurança, ela também possui um sistema de avaliação de usuário, permitindo verificar um pouco das características dos confirmados.

Dessa forma, o projeto se diferencia ao oferecer uma solução que não foca apenas em eventos e temas específicos, mas sim ao cotidiano dos usuários, fortalecendo as interações sociais e as organizações dessas atividades.

---

## 1.4 Público-Alvo

O projeto possui 2 públicos alvos possíveis, o participante e o organizador do evento.

O participante pode ser qualquer pessoa que estiver logada no sistema, podendo visualizar e participar de eventos que ela escolher.

Assim como o participante, o organizador também pode ser qualquer pessoa, onde é possível criar seus eventos com informações para que os participantes possam acompanhar e entender o que está sendo criado.

A plataforma foi pensada para ser acessível e intuitiva, não exigindo conhecimentos técnicos prévios, para assim, facilitar a adesão e utilização por diferentes perfis de usuários.


---

## 1.5 Objetivos do Projeto

### Objetivo Geral

O principal objetivo do projeto é acabar com a dificuldade de organizar e criar atividades e eventos informais por falta de participantes, assim como também ajudar pessoas com dificuldade de socialização e novatos nas cidades a encontrarem atividades de lazer do dia a dia com pessoas que não são do mesmo convívio, criando novas amizades e experiências.

---

### Objetivos Específicos

- Permitir a criação e gerenciamento de eventos informais de forma simples e rápida;
- Facilitar a descoberta de eventos com base em localização e interesse dos usuários;
- Possibilitar a participação dinâmica em eventos, com controle de vagas disponíveis;
- Implementar um sistema de avaliação de usuários para aumentar a confiança nas interações;
- Promover a conexão social entre pessoas por meio de atividades presenciais.


---

## 1.6 Métricas de Sucesso (KPIs)

- Tempo médio de resposta inferior a 300ms nas principais operações;
- Suporte a pelo menos 50 usuários simultâneos sem degradação significativa de desempenho;
- Pelo menos 70% dos eventos criados atingirem o número mínimo de participantes;
- Taxa de participação em eventos superior a 60% dos usuários cadastrados;
- Média de avaliação dos usuários igual ou superior a 4 (em uma escala de 1 a 5).

---

# 2. Engenharia de Requisitos

## 2.1 Personas

## Persona 1 — O Organizador Frustrado

**Nome:** Lucas Andrade

**Idade:** 24 anos

**Contexto:**

Lucas mora em uma cidade de médio porte e gosta de organizar partidas de futebol com amigos. Ele costuma criar grupos no WhatsApp, mas frequentemente tem dificuldades para fechar o número de jogadores.

**Objetivos:**

- Conseguir completar times com facilidade
- Organizar eventos de forma rápida
- Evitar cancelamentos de última hora

**Principais dificuldades:**

- Pessoas desmarcam em cima da hora
- Falta de jogadores suficientes
- Organização bagunçada em grupos de mensagem
- Dificuldade de alcançar novas pessoas fora do círculo de amigos

## Persona 2 — O Participante Interessado

**Nome:** Ana Maria

**Idade:** 22 anos

**Contexto:**

Ana é nova na cidade em que reside, não tem amigos, conhecidos e nem familiares por perto. Ela gosta de fazer caminhadas e exercícios físicos com pelo menos uma pessoa, porém não conhece ninguém para participar com ela, querendo buscar novas amizades para realizar tais atividades.

**Objetivos:**

- Conseguir novas amizades
- Encontrar pessoas procurando por companhia para caminhadas

**Principais dificuldades:**

- Não ter pessoas conhecidas por perto
- Medo de entrar em grupos desconhecidos
- Não conhecer os lugares da cidades para realizar as atividades

## Persona 3 — O Usuário Cauteloso

**Nome:** Rafael Lima

**Idade:** 28 anos

**Contexto:**

Rafael gosta de esportes e eventos sociais, mas já teve experiências ruins com grupos desorganizados e pessoas irresponsáveis.

**Objetivos:**

- Participar de eventos confiáveis
- Evitar experiências negativas
- Garantir segurança e organização

**Principais dificuldades:**

- Falta de confiança nos participantes
- Eventos mal organizados
- Falta de avaliação/reputação dos usuários
- Medo de perder tempo com eventos ruins

---

## 2.2 Casos de Uso Principais

Liste os principais fluxos do sistema.

Exemplo:

- criar conta
- registrar dados
- consultar informações
- gerar relatórios

Sempre que possível inclua **diagramas de caso de uso**.

---

## 2.3 Requisitos Funcionais (RF)

Use a estrutura:

> O sistema deve permitir que **[ator] realize [ação]**.

Exemplo:

RF01 — O sistema deve permitir que o usuário crie uma conta.

RF02 — O sistema deve permitir que o usuário registre informações.

RF03 — O sistema deve permitir que o usuário visualize dados registrados.

---

## 2.4 Requisitos Não Funcionais (RNF)

Inclua requisitos relacionados a:

- desempenho
- segurança
- disponibilidade
- escalabilidade
- usabilidade

Exemplo:

RNF01 — O sistema deve suportar 100 usuários simultâneos.  
RNF02 — O tempo de resposta deve ser inferior a 300ms.  
RNF03 — O sistema deve utilizar autenticação segura.

---

## 2.5 Regras de Negócio

Exemplos:

- apenas usuários autenticados podem acessar determinados recursos
- determinadas operações exigem validação adicional

---

## 2.6 Fora do Escopo

Liste explicitamente **o que o sistema não fará**.

Isso ajuda a evitar crescimento descontrolado do projeto.

---
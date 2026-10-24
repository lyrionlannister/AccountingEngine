# Instrucciones de versionamiento para agentes

Estas reglas aplican a cualquier agente de IA (Copilot incluido) que haga commits, ramas o Pull Requests en este repositorio.

## Commits

- Conventional Commits: `tipo(scope opcional): descripción en imperativo, minúscula, sin punto final`.
  Tipos: `feat`, `fix`, `refactor`, `perf`, `docs`, `style`, `test`, `chore`, `ci`.
- Agrupar cambios por propósito. Si hay cambios de distinta naturaleza (ej. un fix y una feature), van en commits separados, no mezclados.
- **Nunca firmar ni mencionar IA como autor o co-autor.** Sin `Co-Authored-By: Claude`/`Copilot`, sin "Generated with...", sin trailers ni emojis de robot. El commit se ve como si lo hubiera escrito la persona que lo pidió.

## Ramas

- `main` siempre estable. Ningún cambio se commitea directo ahí.
- Nombre de rama: `tipo/nombre-descriptivo` (mismo prefijo que Conventional Commits).
- **Toda rama nace del último commit en `origin/main`, nunca de un `main` local desactualizado.** Antes de crear la rama: `git checkout main && git pull origin main`, recién ahí `git checkout -b`.
- **Nunca abrir una rama nueva si la anterior todavía no está mergeada en `main`.** No alcanza con que tenga PR abierto o esté "lista" — tiene que estar efectivamente en `main`. No se trabaja en paralelo sobre ramas hermanas nacidas del mismo punto.
- Nunca mezclar features distintas en la misma rama.

## Antes de hacer push

- **Nunca hacer `git push` sin antes `git fetch origin`.** Si `origin/<rama>` tiene commits que la copia local no tiene, nunca empujar directo ni forzar: integrar primero (`git pull --rebase origin <rama>`, o merge si el equipo evita rebase en ramas compartidas), resolver lo que haga falta, y recién entonces `git push`.
- Nunca rebase de ramas compartidas sin permiso explícito de quien la pidió.

## Pull Requests

- El agente puede crear el PR (`gh pr create` o equivalente) y revisar el estado del CI.
- **El agente nunca mergea el PR.** El merge es exclusivo de la persona dueña del repositorio, sin excepción — ni con CI en verde, ni si esa persona está ausente. El trabajo del agente termina al reportar el estado del PR/CI.

## Tags y releases

SemVer `MAJOR.MINOR.PATCH`: `MAJOR` cambios incompatibles, `MINOR` funcionalidad nueva compatible, `PATCH` corrección de bugs compatible.

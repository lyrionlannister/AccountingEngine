-- Bootstrap manual del primer PlatformAdministrator.
--
-- GrantPlatformAdministratorService exige que quien otorga el rol ya sea
-- PlatformAdministrator (ver Companies.Services.GrantPlatformAdministratorService),
-- lo que hace imposible crear al primero a través del Service: nadie lo es todavía.
-- Por eso el primero se crea a mano, una sola vez por entorno, con este script.
--
-- Requisito previo: el usuario ya debe existir en `usuarios` (p. ej. registrado
-- a través de RegisterUserService).
--
-- Uso: reemplazar el correo y ejecutar contra la base del entorno correspondiente.

INSERT INTO administradores_plataforma (usuario_id, otorgado_en)
SELECT id, now()
FROM usuarios
WHERE correo = 'reemplazar-con-el-correo-del-primer-administrador@dominio.com'
ON CONFLICT DO NOTHING;

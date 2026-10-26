INSERT INTO administradores_plataforma (usuario_id, otorgado_en)
SELECT id, now()
FROM usuarios
WHERE correo = 'reemplazar-con-el-correo-del-primer-administrador@dominio.com'
ON CONFLICT DO NOTHING;

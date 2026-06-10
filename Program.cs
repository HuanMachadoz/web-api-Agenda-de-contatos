using System.ComponentModel.DataAnnotations;
using AgendaContatos.Data;
using AgendaContatos.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
                  ?? "Data Source=agenda.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

static (bool ok, List<string> erros) Validar(Contato c)
{
    var ctx = new ValidationContext(c);
    var resultados = new List<ValidationResult>();
    var ok = Validator.TryValidateObject(c, ctx, resultados, true);
    var erros = resultados.Select(r => r.ErrorMessage ?? "Erro de validação").ToList();

    if (c.DataNascimento.Date > DateTime.Today)
    {
        ok = false;
        erros.Add("A data de nascimento não pode ser no futuro.");
    }
    if (c.Idade > 130)
    {
        ok = false;
        erros.Add("Data de nascimento inválida (idade superior a 130 anos).");
    }
    return (ok, erros);
}

var grupo = app.MapGroup("/api/contatos");
//listar todos
grupo.MapGet("/", async (AppDbContext db) =>
{
    var lista = await db.Contatos.AsNoTracking().ToListAsync();
    return Results.Ok(lista.Select(c => new
    {
        c.Id, c.Nome, c.Email, c.Telefone, c.DataNascimento, c.Idade
    }));
});
//buscar por um
grupo.MapGet("/{id:int}", async (int id, AppDbContext db) =>
{
    var c = await db.Contatos.FindAsync(id);
    if (c is null) return Results.NotFound(new { mensagem = $"Contato {id} não encontrado." });
    return Results.Ok(new { c.Id, c.Nome, c.Email, c.Telefone, c.DataNascimento, c.Idade });
});
//criar
grupo.MapPost("/", async (Contato novo, AppDbContext db) =>
{
    var (ok, erros) = Validar(novo);
    if (!ok) return Results.BadRequest(new { erros });

    if (await db.Contatos.AnyAsync(x => x.Email == novo.Email))
        return Results.Conflict(new { mensagem = "Já existe um contato com este e-mail." });

    novo.Id = 0;
    db.Contatos.Add(novo);
    await db.SaveChangesAsync();
    return Results.Created($"/api/contatos/{novo.Id}",
        new { novo.Id, novo.Nome, novo.Email, novo.Telefone, novo.DataNascimento, novo.Idade });
});
//atualizar
grupo.MapPut("/{id:int}", async (int id, Contato dados, AppDbContext db) =>
{
    var existente = await db.Contatos.FindAsync(id);
    if (existente is null) return Results.NotFound(new { mensagem = $"Contato {id} não encontrado." });

    dados.Id = id;
    var (ok, erros) = Validar(dados);
    if (!ok) return Results.BadRequest(new { erros });

    if (await db.Contatos.AnyAsync(x => x.Email == dados.Email && x.Id != id))
        return Results.Conflict(new { mensagem = "Já existe outro contato com este e-mail." });

    existente.Nome = dados.Nome;
    existente.Email = dados.Email;
    existente.Telefone = dados.Telefone;
    existente.DataNascimento = dados.DataNascimento;

    await db.SaveChangesAsync();
    return Results.Ok(new { existente.Id, existente.Nome, existente.Email, existente.Telefone, existente.DataNascimento, existente.Idade });
});
//apagar
grupo.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var c = await db.Contatos.FindAsync(id);
    if (c is null) return Results.NotFound(new { mensagem = $"Contato {id} não encontrado." });
    db.Contatos.Remove(c);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run(); //server no ar

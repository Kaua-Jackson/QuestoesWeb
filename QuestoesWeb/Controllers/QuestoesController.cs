using Microsoft.AspNetCore.Mvc;
using QuestoesWeb.Models;
using QuestoesWeb.Services;

namespace QuestoesWeb.Controllers;

public sealed class QuestoesController(IQuestoesService questoesService) : Controller
{
    private readonly IQuestoesService _questoesService = questoesService;
    private const string SessionKeyQuestoesAleatorias = "QuestoesAleatorias";

    // GET /Questoes?ordem=1 ou /Questoes?aleatorio=true&indice=0
    [HttpGet]
    public async Task<IActionResult> Index(
        int? ordem,
        bool aleatorio = false,
        int? indice = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<QuestaoViewModel> questoes;
        QuestaoViewModel? questao;

        if (aleatorio)
        {
            // Recupera ou cria ordem aleatória na sessão
            var questoesAleatoriasIds = HttpContext.Session.GetString(SessionKeyQuestoesAleatorias);
            
            if (string.IsNullOrEmpty(questoesAleatoriasIds))
            {
                var questoesAleatorias = await _questoesService.ObterQuestoesAleatoriasAsync(cancellationToken);
                var ids = questoesAleatorias.Select(q => q.Ordem.ToString()).ToList();
                HttpContext.Session.SetString(SessionKeyQuestoesAleatorias, string.Join(",", ids));
                questoes = questoesAleatorias;
            }
            else
            {
                var ids = questoesAleatoriasIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();
                var todasQuestoes = await _questoesService.ObterQuestoesAsync(cancellationToken);
                questoes = ids.Select(id => todasQuestoes.First(q => q.Ordem == id)).ToList();
            }

            var indiceAtual = indice ?? 0;
            questao = await _questoesService.ObterQuestaoPorIndiceAsync(indiceAtual, questoes, cancellationToken);

            if (questao is null)
            {
                return NotFound();
            }

            ViewBag.TotalQuestoes = questoes.Count;
            ViewBag.IndiceAtual = indiceAtual;
            ViewBag.Aleatorio = true;
        }
        else
        {
            var ordemAtual = ordem ?? 1;
            questao = await _questoesService.ObterQuestaoPorOrdemAsync(ordemAtual, cancellationToken);

            if (questao is null)
            {
                return NotFound();
            }

            ViewBag.TotalQuestoes = (await _questoesService.ObterQuestoesAsync(cancellationToken)).Count;
            ViewBag.OrdemAtual = ordemAtual;
            ViewBag.Aleatorio = false;
        }

        return View(questao);
    }

    // POST /Questoes/Responder
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Responder(
        int? ordem,
        bool aleatorio = false,
        int? indice = null,
        int? respostaSelecionadaId = null,
        CancellationToken cancellationToken = default)
    {
        QuestaoViewModel? questao;

        if (aleatorio)
        {
            var questoesAleatoriasIds = HttpContext.Session.GetString(SessionKeyQuestoesAleatorias);
            
            if (string.IsNullOrEmpty(questoesAleatoriasIds))
            {
                return RedirectToAction(nameof(Index), new { aleatorio = true, indice = 0 });
            }

            var ids = questoesAleatoriasIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();
            var todasQuestoes = await _questoesService.ObterQuestoesAsync(cancellationToken);
            var questoes = ids.Select(id => todasQuestoes.First(q => q.Ordem == id)).ToList();

            var indiceAtual = indice ?? 0;
            questao = await _questoesService.ObterQuestaoPorIndiceAsync(indiceAtual, questoes, cancellationToken);

            if (questao is null)
            {
                return NotFound();
            }

            ViewBag.TotalQuestoes = questoes.Count;
            ViewBag.IndiceAtual = indiceAtual;
            ViewBag.Aleatorio = true;
        }
        else
        {
            var ordemAtual = ordem ?? 1;
            questao = await _questoesService.ObterQuestaoPorOrdemAsync(ordemAtual, cancellationToken);

            if (questao is null)
            {
                return NotFound();
            }

            ViewBag.TotalQuestoes = (await _questoesService.ObterQuestoesAsync(cancellationToken)).Count;
            ViewBag.OrdemAtual = ordemAtual;
            ViewBag.Aleatorio = false;
        }

        var selecionada = questao.Respostas.FirstOrDefault(r => r.IdResposta == respostaSelecionadaId);

        var questaoComResposta = questao with
        {
            RespostaSelecionadaId = respostaSelecionadaId,
            EstaCorreta = selecionada?.EhCorreta
        };

        return View("Index", questaoComResposta);
    }
}
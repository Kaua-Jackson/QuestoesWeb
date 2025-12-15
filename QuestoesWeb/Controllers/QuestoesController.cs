using Microsoft.AspNetCore.Mvc;
using QuestoesWeb.Models;
using QuestoesWeb.Services;

namespace QuestoesWeb.Controllers;

public sealed class QuestoesController(IQuestoesService questoesService) : Controller
{
    private readonly IQuestoesService _questoesService = questoesService;

    // GET /Questoes?ordem=1
    [HttpGet]
    public async Task<IActionResult> Index(int ordem = 1, CancellationToken cancellationToken = default)
    {
        var questao = await _questoesService.ObterQuestaoPorOrdemAsync(ordem, cancellationToken);

        if (questao is null)
        {
            return NotFound();
        }

        ViewBag.TotalQuestoes = (await _questoesService.ObterQuestoesAsync(cancellationToken)).Count;

        return View(questao);
    }

    // POST /Questoes/Responder
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Responder(
        int ordem,
        int? respostaSelecionadaId,
        CancellationToken cancellationToken = default)
    {
        var questao = await _questoesService.ObterQuestaoPorOrdemAsync(ordem, cancellationToken);

        if (questao is null)
        {
            return NotFound();
        }

        var selecionada = questao.Respostas.FirstOrDefault(r => r.IdResposta == respostaSelecionadaId);

        var questaoComResposta = questao with
        {
            RespostaSelecionadaId = respostaSelecionadaId,
            EstaCorreta = selecionada?.EhCorreta
        };

        ViewBag.TotalQuestoes = (await _questoesService.ObterQuestoesAsync(cancellationToken)).Count;

        return View("Index", questaoComResposta);
    }
}



//Panel Login
const panelLogin = document.querySelector(".login-form");
const inputUser = document.querySelector("[name='usuario']");
const inputSenha = document.querySelector("[name='senha']");
const inputSenhaRecupera = document.getElementById("SenhaConfirmar");
const erroMessage = document.querySelector(".erro-login");
const linkRecupera = document.querySelector(".recupera-senha-link");
// Btns showHide password
const btnsShow = document.querySelectorAll(".show-password");
// Panel Recupera
const panelRecupera = document.querySelector(".recupera-login");
const formRecuperar = document.querySelector(".recupera-login form");
const mensagemRecupera = document.querySelector(".mensagem-recupera");
const formAlterarSenha = document.getElementById("formAlterarSenha");
const btnVoltar = document.querySelector(".volta-login");
const btnVoltarRecuperar = document.querySelector(".btnVoltarRecuperar");
const btnSubmitRecuperar = document.querySelector(".envia-recupera");
const btnSubmitRecuperarText = btnSubmitRecuperar.querySelector("span");
const btnSubmitRecuperarIcon = btnSubmitRecuperar.querySelector("i");

function checkCharNome(e) {
    const char = String.fromCharCode(e.keyCode);
    const padrao = "^[A-Za-z]$";
    if (char.match(padrao))
        return true;
}

function switchPadrao(padrao) {
    let saidaPadrao;
    switch (padrao) {
        case "user":
            saidaPadrao = "^[A-Za-z]{5,20}$";
            break;
        case "senha":
            saidaPadrao = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z0-9]).{6,}$";
            break;
        default:
            saidaPadrao = "";
            break;
    }
    return saidaPadrao;
}

function GetCheckCorreto(input) {
    var parent = input.parentNode;
    var correto = parent.querySelector('.fa-check');
    return correto;
}

function GetCheckIncorreto(input) {
    var parent = input.parentNode;
    var incorreto = parent.querySelector('.fa-xmark');
    return incorreto;
}

function ErroLoginLoad() {
    if (erroMessage.textContent != ""){
        InputWrong(inputUser);
        InputWrong(inputSenha);
        if (inputSenhaRecupera) InputWrong(inputSenhaRecupera);
    } 
}

function PadraoInput(input, padraoinput) {
    let padrao = switchPadrao(padraoinput);
    if (!input.value.match(padrao)) {
        InputWrong(input);
    }
    else {
        InputCorrect(input);
    }
}

function InputWrong(input) {
    input.style.borderColor = "red";
    input.style.color = "red";
    input.style.setProperty("animation", "shake .3s");
    var checkCorreto = GetCheckCorreto(input);
    var checkIncorreto = GetCheckIncorreto(input);
    checkIncorreto.style.setProperty('display', 'block');
    checkCorreto.style.setProperty('display', 'none');
    $(input).parent().next('.aviso').attr('data-hidden', false)
}

function InputCorrect(input) {
    input.style.borderColor = "limegreen";
    input.style.color = "rgb(82, 82, 82)";
    input.style.setProperty("animation", "none");
    var checkCorreto = GetCheckCorreto(input);
    var checkIncorreto = GetCheckIncorreto(input);
    checkIncorreto.style.setProperty('display', 'none');
    checkCorreto.style.setProperty('display', 'block');
    $(input).parent().next('.aviso').attr('data-hidden', true)
}

function InputTXT() {
    try {
        inputUser.addEventListener("keypress", (e) => {
            if (!checkCharNome(e))
                e.preventDefault();
        });
        inputUser.addEventListener("change", (e) => {
            PadraoInput(inputUser, "user");
        });
        inputSenha.addEventListener("change", (e) => {
            PadraoInput(inputSenha, "senha");
        });
        inputSenhaRecupera.addEventListener("change", (e) => {
            PadraoInput(inputSenhaRecupera, "senha");
        });
    } catch (e) {

    }
}

function InputSenha(input) {
    PadraoInput(input, "senha");
}

function InputSenhaConparar(input, idComparar) {
    if ($(input).val() != $(idComparar).val())
        InputCorrect(input);
    else
        InputWrong(input);
}

function showHidePassword() {
    btnsShow.forEach(x => x.addEventListener('click', (e) => {
        var input = x.parentNode.querySelector('input');
        var show = x.getAttribute('data-show') === "true";
        if (show) {
            x.setAttribute('data-show', false);
            x.classList.remove('fa-eye-slash');
            x.classList.add('fa-eye');
            input.type = "password";
        }
        else {
            x.setAttribute('data-show', true);
            x.classList.remove('fa-eye');
            x.classList.add('fa-eye-slash');
            input.type = "text";
        }
    }));
}

function MostraRecupera(){
    panelLogin.style.setProperty("display", "none");
    panelRecupera.style.setProperty("display", "block");
}

function formRecuperarSubmit() {
    if (!formRecuperar) return;
    formRecuperar.addEventListener("submit", function (e) {
        btnSubmitRecuperar.disabled = true;
        btnVoltar.disabled = true;
        btnSubmitRecuperarText.textContent = "Enviando...";
        btnSubmitRecuperarIcon.dataset.hidden = false;
    });
}

function formAlterarSenhaSubmit() {
    if (!formAlterarSenha) return;
    formAlterarSenha.addEventListener("submit", function (e) {
        btnSubmitRecuperar.disabled = true;
        btnVoltarRecuperar.disabled = true;
        btnSubmitRecuperarText.textContent = "Alterando...";
        btnSubmitRecuperarIcon.dataset.hidden = false;
    })
}

function windowLoadFunc() {
    try {
        window.addEventListener("load", (e) => {
            ErroLoginLoad();
            if (mensagemRecupera && mensagemRecupera.textContent != "") {
                MostraRecupera();
            }
        });
    } catch (e) {

    }
}

function linkRecuperaFunc() {
    try {
        linkRecupera.addEventListener("click", (e) => {
            MostraRecupera();
        });
    } catch (e) {

    }
}

function btnVoltarFunc() {
    try {
        btnVoltar.onclick = function () {
            const inputRecupera = panelRecupera.getElementsByTagName("email");
            inputRecupera.textContent = "";
            inputRecupera.value = "";
            panelLogin.style.setProperty("display", "block");
            panelRecupera.style.setProperty("display", "none");
        };
    } catch (e) {

    }
}

windowLoadFunc();
formRecuperarSubmit();
formAlterarSenhaSubmit();
InputTXT();
btnVoltarFunc();
linkRecuperaFunc();
showHidePassword();
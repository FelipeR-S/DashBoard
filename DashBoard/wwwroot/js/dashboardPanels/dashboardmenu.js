const modalClass = new modal();

//Controla o menu
const Menu = {
    root: document.documentElement,
    menu: document.querySelector(".dash__menu"),
    panelMenu: document.querySelector(".dash__menu"),
    hamburguer: document.querySelector(".nav__toggle"),
    navList: document.querySelector(".nav__list"),
    btnExpand: document.querySelector(".dash__menu--action"),
    userDados: document.querySelector(".user__dados"),
    // Exibe ou esconde menu no formato mobile
    ControlBtnHamburguer(){
        this.hamburguer.addEventListener("click", (e) => {
            let MenuOpen = this.hamburguer.getAttribute('aria-expanded') === "true";
            //Fecha o menu
            if (MenuOpen) {
                this.hamburguer.setAttribute('aria-expanded', 'false');
                this.navList.setAttribute('data-state', 'closing');
            
                this.navList.addEventListener('animationend', () => {
                    this.navList.setAttribute('data-state', 'closed');
                }, { once: true });
            }
            //Abre o menu
            else{
                this.hamburguer.setAttribute('aria-expanded', 'true');
                this.navList.setAttribute('data-state', 'opened');
            }
        });
    },
    //Expande ou retrai o menu em telas maiores
    ControlBtnMenu(){
        this.btnExpand.addEventListener("click", (e) => {
            let menuExpand = this.menu.getAttribute('aria-expanded') === "true";
            if (menuExpand) this.menu.setAttribute('aria-expanded', 'false');
            else this.menu.setAttribute('aria-expanded', 'true');
            this.ChangeConteudoLeft();
        });
    },
    //Set em atributos ao retrair menu em telas maiores
    ChangeConteudoLeft(){
        this.userDados.addEventListener('animationend', () => {
            //Aumenta ou diminui o espaço para o painel de conteúdo da página
            let newWidth = this.panelMenu.offsetWidth;
            this.root.style.setProperty("--medida-menu-wide", `${newWidth}px`);
        }, { once: true });
    }
}

//Controla a imagem de usuário
const Image = {
    userImage: document.querySelector('.user__img'),
    userImageConfig: document.querySelector('.user__img--config'),
    //Carrega imagem atual ou default
    LoadUserImage() {
        let userMatricula = document.querySelector('.user__matricula').innerHTML.replace("Matrícula: ", "");
        let file = `../img/dashboard/user/${userMatricula}.jpg`;
        let xhttp = new XMLHttpRequest();
        
        //Request da imagem de usuário
        xhttp.onreadystatechange = function () {
            if (this.readyState == 4) {
                if (this.status == 200)
                    Image.userImage.style.setProperty("background-image", `url(../../img/dashboard/user/${userMatricula}.jpg?nocache=${Math.random()})`);
                if (this.status == 404)
                    Image.userImage.style.setProperty("background-image", `url(../../img/dashboard/user/default.webp?nocache=${Math.random()})`);
            }
        }
        xhttp.open("GET", `${file}`, true);
        xhttp.setRequestHeader("Cache-Control", "no-cache, no-store, max-age=0");
        xhttp.send();
        
        //Carrega configuração da imagem no localStorage
        let bgPosition = localStorage.getItem("backgroundPosition");
        let bgSize = localStorage.getItem("backgroundSize");
        
        //Set das configurações na imagem de usuário
        this.userImageConfig.style.setProperty("background-position", bgPosition);
        this.userImageConfig.style.setProperty("background-size", bgSize);
    },
    //Recarrega a imagem ao fechar dialog  
    ImageReload(){
        let dialog = document.querySelector('.layout__dialog');
        dialog.addEventListener("close", (e) => {
            this.LoadUserImage();
        })
    }
}

//Deixa as configurações em default durante resize
window.addEventListener('resize', () => {
    Menu.hamburguer.setAttribute('aria-expanded', 'false');
    Menu.navList.setAttribute('data-state', 'closed');
    Menu.ChangeConteudoLeft();
});

window.addEventListener('load', () => {
    Menu.ChangeConteudoLeft();
    Image.LoadUserImage();
});

Menu.ControlBtnHamburguer();
Menu.ControlBtnMenu();
Image.ImageReload();
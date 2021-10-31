let store=new Vuex.Store({strict:!0,state:{selectedImages:[],errMsg:""},mutations:{setSelectedImages(a,b){a.selectedImages=b},addSelectedImage(a,b){a.selectedImages.push(b)},removeSelectedImage(a,b){a.selectedImages.splice(b,1)},setErrMsg(a,b){b?a.errMsg+=b:a.errMsg=b}},actions:{selectImage:function({commit:a},b){a("addSelectedImage",b)},deselectImage:function({commit:a},b){a("removeSelectedImage",b)},emptySelectedImages({commit:a}){a("setSelectedImages",[])},setErrMsg({commit:a},b){a("setErrMsg",b)},emptyErrMsg({commit:a}){a("setErrMsg","")}}});Vue.component("blog-media",{template:"#blog-media-template",mixins:[mediaMixin],props:["mode"],store,data:()=>({editDialogVisible:!1,progressDialog:!1,pageNumber:1,selectedImageIdx:0,totalFileCount:0,isEditor:!1,fileInput:null}),mounted(){this.initWindowDnd(),this.initFileInput(),"editor"===this.mode&&(this.isEditor=!0,this.initImages())},computed:{showMoreVisible:function(){return this.count>this.images.length},leftArrowVisible:function(){return 1<this.selectedImages.length&&0<this.selectedImageIdx},rightArrowVisible(){return 1<this.selectedImages.length&&this.selectedImageIdx<this.selectedImages.length-1},selectedImages(){return this.$store.state.selectedImages},errMsg(){return this.$store.state.errMsg}},methods:{initWindowDnd(){window.addEventListener("dragenter",function(){document.querySelector("#dropzone").style.visibility="",document.querySelector("#dropzone").style.opacity=1}),window.addEventListener("dragleave",function(a){a.preventDefault(),document.querySelector("#dropzone").style.visibility="hidden",document.querySelector("#dropzone").style.opacity=0}),window.addEventListener("dragover",function(a){a.preventDefault(),document.querySelector("#dropzone").style.visibility="",document.querySelector("#dropzone").style.opacity=1});let a=this;window.addEventListener("drop",function(b){b.preventDefault(),document.querySelector("#dropzone").style.visibility="hidden",document.querySelector("#dropzone").style.opacity=0,a.dragFilesUpload(b.dataTransfer.files)})},dragFilesUpload(a){a.length&&this.sendImages(this.getFormData(a))},initFileInput(){let a=this;this.fileInput=document.getElementById("fileInput"),this.fileInput.addEventListener("change",function(){a.sendImages(a.getFormData(a.fileInput.files))},!1)},chooseFilesUpload(){this.fileInput.click()},getFormData(a){this.progressDialog=!0;const b=new FormData;this.totalFileCount=a.length,this.$store.dispatch("emptyErrMsg");let c=0,d=0,e=[];for(let b=0;b<this.totalFileCount;b++){let f=a[b],g=!1;this.validFileTypes.forEach(a=>{if(f.name.substr(f.name.length-a.length,a.length).toLowerCase()===a.toLowerCase())return void(g=!0)}),g?f.size<=this.maxImageFileSize?e.push(f):d++:c++}return e.forEach(a=>b.append("images",a)),0<c&&this.$store.dispatch("setErrMsg",this.errFileType+" "),0<d&&this.$store.dispatch("setErrMsg",this.errFileSize+" "),b},sendImages(a){axios.post("/admin/media?handler=image",a,this.$root.headers).then(a=>{let b=a.data.images.length;0<b&&(a.data.images.forEach(a=>this.images.unshift(a)),this.count+=b,this.$root.toast("Image uploaded.")),0<a.data.errorMessages.length&&a.data.errorMessages.forEach(a=>this.$store.dispatch("setErrMsg",a+" ")),this.errMsg&&this.$store.dispatch("setErrMsg",`${b} of ${this.totalFileCount} files were uploaded.`),this.progressDialog=!1}).catch(()=>{this.progressDialog=!1,this.$root.toastError("Image upload failed.")})},insertImages(){this.$root.insertImages()},initImages(){axios.get("/admin/media?handler=images").then(a=>{this.images=a.data.medias,this.count=a.data.count}).catch(()=>void 0)},clickImage(a){let b=this.selectedImages.findIndex(b=>b.id===a.id);-1===b?(a.selected=!0,this.$store.dispatch("selectImage",a)):(a.selected=!1,this.$store.dispatch("deselectImage",b))},leftArrow(){this.selectedImageIdx--},rightArrow(){this.selectedImageIdx++},showMore(){this.pageNumber++,this.images.length<this.pageSize&&this.pageNumber--;let a=`/admin/media?handler=more&pageNumber=${this.pageNumber}`;axios.get(a).then(a=>{for(var b,c=0;c<a.data.length;c++)b=this.images.some(function(b){return b.id===a.data[c].id}),b||this.images.push(a.data[c])}).catch(()=>void 0)},editImages(){this.editDialogVisible=!0},deleteImages(){if(confirm("Are you sure you want to delete the image(s)? They will no longer appear anywhere on your website. This cannot be undone!")){this.$store.dispatch("emptyErrMsg");const b=this.selectedImages.length;let c=[];for(var a=0;a<b;a++)c.push(this.selectedImages[a].id);axios.post("/admin/media?handler=delete",c,this.$root.headers).then(()=>{for(var a=0;a<b;a++){let b=this.images.findIndex(b=>b.id===this.selectedImages[a].id);this.images.splice(b,1)}this.$store.dispatch("emptySelectedImages"),this.count-=b,this.$root.toast("Image deleted.")}).catch(()=>{this.$root.toastError("Image delete failed.")})}},updateImage(){axios.post("/admin/media?handler=update",this.selectedImages[this.selectedImageIdx],this.$root.headers).then(()=>{this.$root.toast("Image updated.")}).catch(()=>{this.$root.toastError("Image update failed.")})},closeEditDialog(){this.selectedImageIdx=0,this.editDialogVisible=!1},closeMediaDialog(){this.$root.closeMediaDialog()}}});
//# sourceMappingURL=blog-media.js.map
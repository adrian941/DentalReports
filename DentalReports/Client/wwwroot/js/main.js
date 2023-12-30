


window.isAnimationRunning = true;
window.initialize3DViewer = function (fileSourcesArray) {

	var element = document.querySelector('.big-container');
	if (element) {
		element.remove();
	}

	window.spanLoading = document.getElementById('loading-span');
	spanLoading.style.display = 'visible';
	spanLoading.style.zIndex = 50;
	spanLoading.textContent = 'Loading 3D Models...';

	async function init() {

		
	 
		async function initMaterials() {
			// When UPDATE THIS , update window.MaterialsJson too
	
			window.matCraniu = new THREE.MeshPhongMaterial({
				color: 0xffffff,
				opacity: 1,
				transparent: true,
				shininess: 0.1,
				specular: 0x000000,
				emissive: 0x282828,
				flatShading: false,
				side: THREE.DoubleSide,
				alphaTest: 0.01,
			});
			window.matImplant = new THREE.MeshPhongMaterial({
				color: 0xa258ec,
				emissive: 0x000000,
				specular: 0x111111,
				opacity: 1,
				transparent: true,
				reflectivity: 0.2,
			
				premultipliedAlpha: false,
				side: THREE.DoubleSide,
				alphaTest: 0.01,
				depthWrite: true,
	
				flatShading: true
	
			});
			window.matCapa = new THREE.MeshPhongMaterial({
				color: 0x1f7a74,
				emissive: 0x000000,
				specular: 0x080808,
				opacity: 1,
				transparent: true,
				
				reflectivity: 0.2,
				
				premultipliedAlpha: false,
				side: THREE.DoubleSide,
				alphaTest: 0.01,
				depthWrite: true,
				flatShading: false
	
			});
			window.matSurub = new THREE.MeshPhongMaterial({
				color: 0x5977ee,
				emissive: 0x000000,
				specular: 0x111111,
				opacity: 1,
				transparent: true,
				
				reflectivity: 0.2,
				
				premultipliedAlpha: false,
				side: THREE.DoubleSide,
				alphaTest: 0.01,
				depthWrite: true,
				flatShading: false
	
			});
			window.matModel = new THREE.MeshPhongMaterial({
	
				color: 0xc78c5c, // Portocaliu deschis
				emissive: 0x272725,
				specular: 0x030401,
				opacity: 1,
				transparent: true,
				alphaTest: 0.01,
			
				side: THREE.DoubleSide,
			});
			window.matAntagonist = new THREE.MeshPhongMaterial({
	
				color: 0xa9cfea, // Portocaliu deschis
				emissive: 0x1b1d1d,
				specular: 0x040404,
				opacity: 1,
				transparent: true,
				alphaTest: 0.01,
				side: THREE.DoubleSide,
			});
			window.matGingie = new THREE.MeshPhongMaterial({
	
				color: 0xfd6895,  
				emissive: 0x100f0f,
				specular: 0x0b0b0b,
				opacity: 1,
				transparent: true,
				alphaTest: 0.01,
				side: THREE.DoubleSide,
			});
			window.matWaxUp = new THREE.MeshPhongMaterial({
				color: 0x9ca0b0,
				emissive: 0x1f1f1f,
				specular: 0x0f0f0f,
				opacity: 1,
				transparent: true,
				premultipliedAlpha: false,
				side: THREE.DoubleSide,
				alphaTest: 0.01,
				depthWrite: true,
				flatShading: false,
			});
		}
		 
		async function initScene() {
			const canvas = document.getElementById('canvas-webgl');

			window.meshes = [];

			window.sizes = {
				width: window.innerWidth,
				height: window.innerHeight
			}
			window.objects3D = null;
			window.cameraPosition = null;
			window.cameraRotation = null;
	
			window.scene = new THREE.Scene();
	
			window.scene.background = new THREE.Color(0x20204d);
			window.renderer = new THREE.WebGLRenderer({
				canvas: canvas,
				antialias: true,
				alpha: true
			});
			
			renderer.outputEncoding = THREE.sRGBEncoding
			renderer.logarithmicDepthBuffer = true
			renderer.shadowMap.enabled = true
			renderer.shadowMap.type = THREE.PCFSoftShadowMap
			renderer.setSize(sizes.width, sizes.height)
			renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2))
			renderer.setClearColor(0x2e2e2e)
			
			window.camera = new THREE.PerspectiveCamera(5, sizes.width / sizes.height, 40, 10000);
			
			camera.position.set(1800, 2, 20);
			scene.add(camera);

			const directionalLight = new THREE.DirectionalLight(0xffffff, 1);
			directionalLight.shadow.mapSize.set(1024, 1024);
			camera.add(directionalLight)
	
			const stlLoader = new THREE.STLLoader();
			window.stlLoader = stlLoader;
	
			window.controls = new THREE.ArcballControls(camera, canvas, scene);
			controls.setGizmosVisible(false);

			//init materials
			 
			
			window.materialsJson = [
				{"Material": window.matWaxUp , "MaterialVariableName" : "matWaxUp"} ,
				{"Material": window.matModel, "MaterialVariableName" : "matModel"}, 
				{"Material": window.matAntagonist, "MaterialVariableName" : "matAntagonist"}, 
				{"Material": window.matCraniu, "MaterialVariableName" : "matCraniu"} , 
				{"Material": window.matImplant, "MaterialVariableName" : "matImplant"}, 
				{"Material": window.matCapa, "MaterialVariableName" : "matCapa"}, 
				{"Material": window.matSurub, "MaterialVariableName" : "matSurub"}, 
				{"Material": window.matGingie, "MaterialVariableName" : "matGingie"}
			];
	
			// fetch from json file : cameraPosition , cameraRotation , objects
			try {

				console.log(fileSourcesArray);
				const response = await fetch(fileSourcesArray[0]);
				if (response.ok) {
					let responseJson = await response.json();
					window.objects3D = responseJson.objects;
					window.cameraPosition = responseJson.cameraPosition;
					window.cameraRotation = responseJson.cameraRotation;
				} else {
					console.error("Eroare în descărcarea fișierului JSON1:", response.status);
				}
			} catch (error) {
				console.error("Eroare în descărcarea fișierului JSON2:", error);
			
			}
			
	
	
			//La final 
			window.addEventListener('resize', () => {
	
				sizes.width = window.innerWidth
				sizes.height = window.innerHeight
			
				camera.aspect = sizes.width / sizes.height
				camera.updateProjectionMatrix()
			
				renderer.setSize(sizes.width, sizes.height)
				renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2))
			});
		}
		async function loadGuiElements() {

		 
		


			var bigContainer = document.createElement('div');
			bigContainer.classList.add('big-container');	
			bigContainer.classList.add('minimized');

			var bigTitle = document.createElement('div');
			bigTitle.className = 'big-title';
			bigTitle.classList.add('minimized');	
			bigTitle.textContent = 'Controls';
			bigTitle.addEventListener('click', function () {	
				bigContainer.classList.toggle('minimized');
				bigTitle.classList.toggle('minimized');
  
				var entries = bigContainer.querySelectorAll('.container-slider');
				entries.forEach(function (entry) {
					entry.classList.toggle('minimized');
				});
			});
			bigContainer.appendChild(bigTitle);
			document.body.appendChild(bigContainer);



			var containerGui = document.createElement('div');
			containerGui.className = 'container-gui';
 
			for (const [key, value] of Object.entries(objects3D)) {

				

				const containerSlider = document.createElement('div');
				containerSlider.id = "container-slider-" + key;
				containerSlider.className = 'container-slider';
				containerSlider.classList.add('minimized');
				bigContainer.appendChild(containerSlider);

				const titleContainer = document.createElement('div');
				titleContainer.className = 'title-container';
				
				const titleText = document.createElement('span');
				titleText.className = 'title-text';
				titleText.textContent = value.DisplayName;
				
				const buttonSlider = document.createElement('button');
				buttonSlider.className = 'btn-slider';
				
				const currentContainerSlider = document.getElementById("container-slider-" + key);
				if(value.Visible){
				  //TODO: Nothing 
				}else {
			 
					buttonSlider.classList.toggle('unchecked');
					 

					currentContainerSlider.style.opacity = 0.5;
				}

		 
				buttonSlider.addEventListener('click', () => {
					buttonSlider.classList.toggle('unchecked');
					if(buttonSlider.classList.contains("unchecked")) {
						currentContainerSlider.style.opacity = 0.5;
						value.Mesh.material.visible = false;
					}else {
						currentContainerSlider.style.opacity = 1;
						value.Mesh.material.visible = true;
					}
				} )


				// Sliders

				const box = document.createElement('div');
				box.className = 'box';

				const range = document.createElement('input');

				
				range.type = 'range';
				range.className = 'range';

				
				

				range.min = '0';
				range.max = '1';
				range.step = '0.01';
				range.value = value.Opacity;

				//const materialColor = "#" + value.Mesh.material.color.getHexString();
				//const rangeId = 'range-' + key.replace(/\./g, "");
				//range.id = rangeId;
				//const cssStyle = `#${rangeId}::-webkit-slider-thumb , #${rangeId}::-webkit-range-thumb{ box-shadow: -405px 0 0 400px ${desaturatedColor};}`;
				//var styleElement = document.createElement("style");
				//styleElement.appendChild(document.createTextNode(cssStyle));
				//document.head.appendChild(styleElement);
				
			  	
				value.Mesh.material.color.getStyle();

				const rangeValue = document.createElement('span');
				rangeValue.className = 'range-value';
				rangeValue.textContent = Math.floor(value.Opacity * 100) + '%';
			 
				range.addEventListener ("input", (e) => {
					
					rangeValue.textContent = Math.floor(e.target.value * 100) + '%';
					
					value.Mesh.material.opacity=e.target.value;

					buttonSlider.classList.remove('unchecked');
			
					currentContainerSlider.style.opacity = 1;
					value.Mesh.material.visible = true;

				}); 
				
			
				box.appendChild(range);
				box.appendChild(rangeValue);
				
				titleContainer.appendChild(buttonSlider);
				titleContainer.appendChild(titleText);
			 
				containerSlider.appendChild(titleContainer);
				containerSlider.appendChild(box);

				 
 
			}


		
			

		}
		async function loadStls() {
		
			
		
			const promises = [];
	
				
			for (const [key, value] of Object.entries(objects3D)) {
				 
				promises.push(new Promise((resolve, reject) => {

					 
	  
				  
					stlLoader.load(
						fileSourcesArray.find(element => element.endsWith(key)),
						function (geometry){
							geometry.computeVertexNormals();
							const searchMaterial = materialsJson.find(element => 
								element.MaterialVariableName === value.Material).Material;

							const material =  new THREE.MeshPhongMaterial(searchMaterial);
						 
							material.opacity = parseFloat(value.Opacity);
							material.visible = value.Visible;
						 

							value.Mesh = new THREE.Mesh(geometry,material);
							meshes.push(value.Mesh); 
							resolve();
						}
					);
				} ));

	
			}
		
		
			return Promise.all(promises);

		}
		async function centerSTLtoScreen() {

			const combinedBoundingBox = new THREE.Box3();
 
			meshes.forEach(mesh => {
				const boundingBox = new THREE.Box3().setFromObject(mesh);
				combinedBoundingBox.union(boundingBox);
			 });
		 
		 
			const center = new THREE.Vector3();
			combinedBoundingBox.getCenter(center);
			let translation = new THREE.Vector3().subVectors(new THREE.Vector3(0, 0, 0), center);
		
		 
			for (let i = 0; i < meshes.length; i++) {
		
				const mesh = meshes[i];
			
				mesh.position.add(translation);
				scene.add(mesh);
		
		
			}
			spanLoading.style.display = 'none';

			camera.position.x = cameraPosition.x;
			camera.position.y = cameraPosition.y;
			camera.position.z = cameraPosition.z;

			camera.rotation.x = cameraRotation.x;
			camera.rotation.y = cameraRotation.y;
			camera.rotation.z = cameraRotation.z;



		}
		function updateRenderOrder() {
			if (meshes == undefined) return;
			meshes.forEach((mesh, index) => {

				mesh.renderOrder = 1 / mesh.material.opacity * 1000;
			 
			});
		}
		function animateScene() {
		
			const animate = () => {
			 
				if (window.isAnimationRunning) {
					
					updateRenderOrder();
					controls.update();
					renderer.render(scene, camera);
					requestAnimationFrame(animate);
				}
			}
			animate();
		}
		async function main() {

		
			
			await initMaterials();
			await initScene();
			await loadStls();
			
			await centerSTLtoScreen();
 
		  
			await loadGuiElements();
			 
		
		  
			
			animateScene();
		}
		main();


		 
	 
	 

	  
	 

	}
	
	
	
	
	
	init();
	}
	
	
	window.initScene = function () {
	 
		window.isAnimationRunning = true;
	
	}

	window.destroy = function () {
	 
		window.isAnimationRunning = false;
		window.meshes = [];

 
		renderer.dispose();
		renderer.forceContextLoss();
 

		var element = document.querySelector('.container-gui');
		if (element) {
	 
			element.remove();
		}


	
	
		window.container = null;
	
	}
	

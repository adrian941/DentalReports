window.container=null;
window.isAnimationRunning = true;
window.meshes = [];
window.initialize3DViewer = function (fileSourcesArray) {
async function init() {

	window.addEventListener('resize', () => {
 
		sizes.width = window.innerWidth
		sizes.height = window.innerHeight
 
		camera.aspect = sizes.width / sizes.height
		camera.updateProjectionMatrix()
 
		renderer.setSize(sizes.width, sizes.height)
		renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2))
	});
	const sizes = {
		width: window.innerWidth,
		height: window.innerHeight
	}


	/*const gui = new dat.GUI()*/

	const canvas = document.querySelector('canvas.webgl');
	const scene = new THREE.Scene();
	const meshes = [];
	window.meshes = meshes;



	scene.background = new THREE.Color(0x20204d);
	const renderer = new THREE.WebGLRenderer({
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

	const camera = new THREE.PerspectiveCamera(5, sizes.width / sizes.height, 40, 10000)
	camera.position.set(1800, 2, 20);
	scene.add(camera);

	const directionalLight = new THREE.DirectionalLight(0xffffff, 1);
	directionalLight.shadow.mapSize.set(1024, 1024);
	camera.add(directionalLight)
 
	const controls = new THREE.ArcballControls(camera, canvas, scene);
	controls.setGizmosVisible(false);
	const matCraniu = new THREE.MeshPhongMaterial({
		color: 0xffffff,
		opacity: 0.06,
		transparent: true,
		shininess: 0.1,
		specular: 0x000000,
		emissive: 0x282828,
		flatShading: false,
		side: THREE.DoubleSide,
		alphaTest: 0.05,
	});
	const matImplant = new THREE.MeshPhongMaterial({
		color: 0xa258ec,
		emissive: 0x000000,
		specular: 0x111111,
		opacity: 1,
		transparent: true,
		metalness: 0.9,
		roughness: 0.2,
		//transmission:0.5,
		reflectivity: 0.2,
		clearcoat: 0,
		clearcoatRoughness: 0,
		premultipliedAlpha: false,
		side: THREE.DoubleSide,
		alphaTest: 0.05,
		depthWrite: true,

		flatShading: true

	})
	const matCapa = new THREE.MeshPhongMaterial({
		color: 0x1f7a74,
		emissive: 0x000000,
		specular: 0x080808,
		opacity: 1,
		transparent: true,
		metalness: 0.9,
		roughness: 0.2,
		//transmission:0.5,
		reflectivity: 0.2,
		clearcoat: 0,
		clearcoatRoughness: 0,
		premultipliedAlpha: false,
		side: THREE.DoubleSide,
		alphaTest: 0.05,
		depthWrite: true,
		flatShading: false

	})
	const matSurub = new THREE.MeshPhongMaterial({
		color: 0x5977ee,
		emissive: 0x000000,
		specular: 0x111111,
		opacity: 1,
		transparent: true,
		metalness: 0.9,
		roughness: 0.2,
		//transmission:0.5,
		reflectivity: 0.2,
		clearcoat: 0,
		clearcoatRoughness: 0,
		premultipliedAlpha: false,
		side: THREE.DoubleSide,
		alphaTest: 0.05,
		depthWrite: true,
		flatShading: false

	})
	const matModel = new THREE.MeshPhongMaterial({

		color: 0xc78c5c, // Portocaliu deschis
		emissive: 0x272725,
		specular: 0x030401,
		opacity: 1,
		transparent: true,
		metalness: 0, // Redus pentru a reduce aspectul metalic
		roughness: 1, // Ridicat pentru a obține o suprafață mai puțin lucioasă

		side: THREE.DoubleSide,
	});
	const matAntagonist = new THREE.MeshPhongMaterial({

		color: 0xa9cfea, // Portocaliu deschis
		emissive: 0x1b1d1d,
		specular: 0x040404,
		opacity: 0.7,
		transparent: true,
		metalness: 0, // Redus pentru a reduce aspectul metalic
		roughness: 1, // Ridicat pentru a obține o suprafață mai puțin lucioasă

		side: THREE.DoubleSide,
	});
	const matGingie = new THREE.MeshPhongMaterial({

		color: 0xfd6895, // Portocaliu deschis
		emissive: 0x100f0f,
		specular: 0x0b0b0b,
		opacity: 1,
		transparent: true,
		metalness: 0, // Redus pentru a reduce aspectul metalic
		roughness: 1, // Ridicat pentru a obține o suprafață mai puțin lucioasă

		side: THREE.DoubleSide,
	});
	const matWaxUp = new THREE.MeshPhongMaterial({
		color: 0x9ca0b0,
		emissive: 0x1f1f1f,
		specular: 0x0f0f0f,

		opacity: 1,
		transparent: true,
		metalness: 0.9,
		roughness: 0.2,
		//transmission:0.5,
		reflectivity: 0.2,
		clearcoat: 0,
		clearcoatRoughness: 0,
		premultipliedAlpha: false,
		side: THREE.DoubleSide,
		alphaTest: 0.05,
		depthWrite: true,
		flatShading: false,


	})


	let jsonMaterials = null;
	const stlLoader = new THREE.STLLoader();

	async function fetchData() {
		console.log(fileSourcesArray[0]);
		try {
			console.log(fileSourcesArray[0]);
			const response = await fetch(fileSourcesArray[0]);
			if (response.ok) {
				jsonMaterials = await response.json();

				// Aici poți continua cu restul codului, deoarece datele sunt disponibile
			} else {
				console.error("Eroare în descărcarea fișierului JSON1:", response.status);
			}
		} catch (error) {
			console.error("Eroare în descărcarea fișierului JSON2:", error);
			console.log(fileSourcesArray[0]);
		}
	}



	function createSliderInputHandler(material) {
		return function () {
			material.opacity = parseFloat(this.value);
		};
	}


	function createButtonClickHandler(material, slider, button) {
		return function () {

			if (material.opacity == 0) {
				material.opacity = slider.value;
				button.style.background = '#0075FF';
			}
			else {
				material.opacity = 0;
				button.style.background = '#FFFFFF';
			}

		};
	}

	function createGuiElement(name, material) {

		//Entry container: 
		var entryContainer = document.createElement('div');
		entryContainer.className = 'entry-gui';
		entryContainer.classList.add('minimized');

		var slider = document.createElement('input');
		slider.type = 'range';
		slider.className = 'slider-gui';
		slider.min = 0;
		slider.max = 1;
		slider.step = 0.01;
		slider.value = material.opacity;

		var button = document.createElement('button');
		button.className = 'button-gui';

		entryContainer.appendChild(button);
		entryContainer.appendChild(slider);

		//EntrySubtitle:
		var entryName = document.createElement('div');
		entryName.className = 'entry-gui';
		entryName.classList.add('minimized');
	
		entryName.id ='subtitle-gui';
		entryName.textContent = name;

		//Add All to container:
		container.appendChild(entryName);
		container.appendChild(entryContainer);

	 
 
		slider.addEventListener('input', createSliderInputHandler(material));

		button.addEventListener('click', createButtonClickHandler(material, slider, button));




	}
	function findMaterial(materialName) { //TODO



		switch (materialName) {
			case "matImplant":
				createGuiElement('Implant', matImplant);
				return matImplant;

			case "matCapa":
				createGuiElement('Cap', matCapa);
				return matCapa;

			case "matCraniu":
				createGuiElement('CBCT', matCraniu);
				return matCraniu;

			case "matAntagonist":
				createGuiElement('Antagonist' , matAntagonist);
				return matAntagonist;

			case "matWaxUp":
				createGuiElement('Wax-Up', matWaxUp);
				return matWaxUp;

			case "matSurub":
				createGuiElement('Screw', matSurub);
				return matSurub;

			case "matGingie":
				createGuiElement('Gingiva', matGingie);
				return matGingie;


			case "matModel":
				createGuiElement('Model', matModel);
				return matModel;

			default:
				return null;
		}
	}
	function findMaterialFromFileName(fileName) {
		for (var i = 0; i < jsonMaterials.length; i++) {
			if (fileName.endsWith(jsonMaterials[i].fileName)) {
				return jsonMaterials[i].materialName;
			}
		}
		// Dacă nu se găsește un material, poți returna un material implicit sau null, după caz.
		return null;
	}

	function loadStls() 
	{

		const promises = [];

		for (var i = 1; i < fileSourcesArray.length; i++) {

			promises.push(

				new Promise((resolve, reject) => {

					let url = fileSourcesArray[i];
					let materialName = findMaterialFromFileName(fileSourcesArray[i]);


					stlLoader.load(
						url,
						function (geometry) {
							geometry.computeVertexNormals();
							const mesh = new THREE.Mesh(geometry, findMaterial(materialName));
							meshes.push(mesh);
							resolve();
						}
					);



				})
			);
		}

 


		return Promise.all(promises);
	}
 

	async function main() {

		var container = document.createElement('div');
		window.container = container;
		container.className = 'container-gui';
		container.classList.add('minimized');	

		var title = document.createElement('div');
		title.className = 'title-gui';
		title.classList.add('minimized');	

		title.textContent = 'Controls';
		container.appendChild(title);
		document.body.appendChild(container);
		title.addEventListener('click', function () {

			container.classList.toggle('minimized');
			title.classList.toggle('minimized');


			// Adaugă sau elimină clasa "minimized" pentru fiecare element de intrare
			var entries = container.querySelectorAll('.entry-gui');
			entries.forEach(function (entry) {
				entry.classList.toggle('minimized');
			});

		});





		await fetchData();
		await loadStls();

		const combinedBoundingBox = new THREE.Box3();
		meshes.forEach(mesh => {
			const boundingBox = new THREE.Box3().setFromObject(mesh);
			combinedBoundingBox.union(boundingBox);
        });
 
 
		const center = new THREE.Vector3();
		combinedBoundingBox.getCenter(center);
		var translation = new THREE.Vector3().subVectors(new THREE.Vector3(0, 0, 0), center);

 
		for (var i = 0; i < meshes.length; i++) {

			const mesh = meshes[i];
		
			mesh.position.add(translation);
			scene.add(mesh);


		}
		// inceput de GUI
		{
 
	


 












		}
		//FINAL DE GUI







		function updateRenderOrder() {

			if (meshes == undefined) return;
			meshes.forEach((mesh, index) => {

				mesh.renderOrder = 1 / mesh.material.opacity * 1000;
				//console.log(mesh.renderOrder);
			});



		}

		window.isAnimationRunning = true
		const animate = () => {
			if (window.isAnimationRunning) {
				updateRenderOrder();
				controls.update();
				renderer.render(scene, camera)
				requestAnimationFrame(animate)
			}
 		}
		animate();

	}

	main();




 




}


init();

}



window.destroy = function () {
	console.log("JS:destroy");
	window.isAnimationRunning = false;
	window.meshes = [];
	
	var element = document.querySelector('.container-gui');
	if (element) {
 
		element.remove();
	}



	window.container = null;

}

window.init = function () {
	console.log("init");
	window.isAnimationRunning = true;

}



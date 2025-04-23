# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
  config.vm.box = "StefanScherer/windows_2019"

  # Map your project folder so the VM can see your scripts
  config.vm.synced_folder ".", "C:/vagrant"

  # Give yourself a GUI window
  config.vm.provider "virtualbox" do |vb|
  vb.gui = true

    # RAM: choose 4096 or 8192
    vb.memory = 8192

    # (Optional) dedicate 2 CPUs for better performance
    vb.cpus = 2

    # VRAM: set to 128 MB so the GUI feels snappy
    vb.customize ["modifyvm", :id, "--vram", "128"]

  end

  config.winrm.username = "vagrant"
  config.winrm.password = "vagrant"
end

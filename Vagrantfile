# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
  config.vm.box = "StefanScherer/windows_2019"

  # Map your project folder so the VM can see your scripts
  config.vm.synced_folder ".", "C:/vagrant"

  # Give yourself a GUI window
  config.vm.provider "virtualbox" do |vb|
    vb.gui = true
    vb.memory = 8192    
  end

  config.winrm.username = "vagrant"
  config.winrm.password = "vagrant"
end

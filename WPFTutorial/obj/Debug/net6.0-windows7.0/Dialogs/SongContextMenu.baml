   M S B A M L   `   `   `  ����G  CMediaManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:MediaManager.DialogsMediaManager.Dialogs  5   	   *    $MediaManager.Dialogs.SongContextMenu   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   4local"clr-namespace:MediaManager.Dialogs     5   	   
������   � AllowsTransparency$
  True��5	   	    � Title$ SongContextMenu��5
   	   $	��160��6!   $	��120��6.    � 
ResizeMode$ NoResize=�6:    � WindowStyle$
 None=�6P   .��5      � 
������  � RowDefinitions 5   
   �� 5      $��65 �6   �� 5      $��* �6   .+�5   
   �� -   5        $��MenuListView��5   M      ContextMenuList#��[  6   $��3��6<   .@�5      �� $��Add Next��6   �� 5      $��
Add To End��6   �� 5      $��Play Single Song��6   $��0,0,0,15q�66   �� 5      $��Edit Metadata��6   �� 5      $��Delete Song��6   5      �� 5   
   $��1��6   $	��Top=�6!   $��10q�69   5      5      
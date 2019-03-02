﻿/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using SeeingSharp.SampleContainer;
using SeeingSharp.Util;

namespace SeeingSharp.UwpSamples
{
    public class SampleViewModel : ViewModelBase
    {
        private BitmapSource m_bitmapSource;
        private Task m_bitmapSourceTask;

        private async Task LoadSampleImageAsync(AssemblyResourceLink sourceLink)
        {
            var source = new BitmapImage();

            using (var randomAccessStream = sourceLink.OpenRead().AsRandomAccessStream())
            {
                await source.SetSourceAsync(randomAccessStream);
            }

            m_bitmapSource = source;

            RaisePropertyChanged(nameof(BitmapSource));
        }

        public SampleViewModel(SampleMetadata sample)
        {
            SampleMetadata = sample;
        }

        public SampleMetadata SampleMetadata { get; }

        public string Name => SampleMetadata.Name;

        public string Group => SampleMetadata.Group;

        public BitmapSource BitmapSource
        {
            get
            {
                if(m_bitmapSource == null && m_bitmapSourceTask == null)
                {
                    var sourceLink = SampleMetadata.TryGetSampleImageLink();
                    if(sourceLink == null) { return null; }

                    m_bitmapSourceTask = LoadSampleImageAsync(sourceLink);
                }
                return m_bitmapSource;
            }
        }
    }
}